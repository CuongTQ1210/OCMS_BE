using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class GradeService : IGradeService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICertificateService _certificateService;
        private readonly ITrainingScheduleRepository _trainingScheduleRepository;
        private readonly IDecisionService _decisionService;
        private readonly ICourseRepository _courseRepository;
        private readonly IProgressTrackingService _progressTrackingService;
        private readonly Lazy<IRequestService> _requestService;

        public GradeService(UnitOfWork unitOfWork, IMapper mapper, ICertificateService certificateService,
            ITrainingScheduleRepository trainingScheduleRepository, IDecisionService decisionService,
            ICourseRepository courseRepository,
            IProgressTrackingService progressTrackingService, Lazy<IRequestService> requestService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _certificateService = certificateService;
            _trainingScheduleRepository = trainingScheduleRepository;
            _decisionService = decisionService;
            _courseRepository = courseRepository;
            _progressTrackingService = progressTrackingService;
            _requestService = requestService;
        }

        #region Get All Grades
        public async Task<IEnumerable<GradeModel>> GetAllAsync()
        {
            var grades = await _unitOfWork.GradeRepository.FindAsync(
                g => true, // Use a predicate that returns a boolean
                g => g.TraineeAssign,
                g => g.TraineeAssign.ClassSubject,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty.Subject,
                g => g.TraineeAssign.ClassSubject.Class.Course,
                g => g.TraineeAssign.Trainee);

            grades = grades.Where(g => g.TraineeAssign?.ClassSubject?.SubjectSpecialty != null)
                   .ToList();

            return _mapper.Map<IEnumerable<GradeModel>>(grades);
        }
        #endregion

        #region Get Grade By Id
        public async Task<GradeModel> GetByIdAsync(string id)
        {
            var grade = await _unitOfWork.GradeRepository.GetAsync(
                g => g.GradeId == id,
                g => g.TraineeAssign,
                g => g.TraineeAssign.ClassSubject,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty,
                g => g.TraineeAssign.ClassSubject.Class.Course,
                g => g.TraineeAssign.Trainee);

            if (grade == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            return _mapper.Map<GradeModel>(grade);
        }
        #endregion

        #region Create Grade
        public async Task<string> CreateAsync(GradeDTO dto, string gradedByUserId)
        {
            var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetAsync(
                t => t.TraineeAssignId == dto.TraineeAssignID,
                t => t.ClassSubject,
                t => t.ClassSubject.SubjectSpecialty,
                t => t.ClassSubject.SubjectSpecialty.Subject,
                t => t.ClassSubject.Class);
            if (traineeAssign == null)
                throw new KeyNotFoundException($"Trainee assignment with ID '{dto.TraineeAssignID}' not found.");

            var subject = traineeAssign.ClassSubject.SubjectSpecialty.Subject;
            var classInfo = traineeAssign.ClassSubject.Class;
            if (subject == null || classInfo == null)
                throw new Exception("Subject or Class not found.");


            var schedule = await _trainingScheduleRepository.GetSchedulesByClassSubjectIdAsync(traineeAssign.ClassSubjectId);
            if (schedule == null)
                throw new InvalidOperationException("ClassSubject does not have any training schedule.");

            // Check if the trainee already has a grade
            var existingGrade = await _unitOfWork.GradeRepository.GetFirstOrDefaultAsync(g => g.TraineeAssignID == traineeAssign.TraineeAssignId);
            if (existingGrade != null)
                throw new InvalidOperationException("Grade for this trainee assignment already exists.");


            var grade = _mapper.Map<Grade>(dto);
            grade.GradeId = $"G-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            grade.GradedByInstructorId = gradedByUserId;
            grade.EvaluationDate = DateTime.UtcNow;
            grade.TraineeAssignID = traineeAssign.TraineeAssignId;

            // Set initial scores to -1
            grade.ParticipantScore = -1;
            grade.AssignmentScore = -1;
            grade.FinalExamScore = -1;
            grade.FinalResitScore = -1;
            grade.TotalScore = -1;
            grade.gradeStatus = GradeStatus.Pending;

            await _unitOfWork.GradeRepository.AddAsync(grade);
            await _unitOfWork.TraineeAssignRepository.UpdateAsync(traineeAssign);
            await _unitOfWork.SaveChangesAsync();

            return grade.GradeId;
        }
        #endregion

        #region Update Grade
        public async Task<bool> UpdateAsync(string id, GradeDTO dto, string gradedByUserId)
        {
            var existing = await _unitOfWork.GradeRepository.GetAsync(
                g => g.GradeId == id,
                g => g.TraineeAssign,
                g => g.TraineeAssign.ClassSubject,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty,
                g => g.TraineeAssign.ClassSubject.Class,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty.Subject);

            if (existing == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            // Ensure the dto trainee assign matches the existing grade's trainee assign
            if (existing.TraineeAssignID != dto.TraineeAssignID)
                throw new InvalidOperationException("Cannot change the trainee assignment for an existing grade.");

            var assignTrainee = existing.TraineeAssign;
            if (assignTrainee == null)
                throw new KeyNotFoundException($"Trainee assignment not found for this grade.");

            var subject = assignTrainee.ClassSubject.SubjectSpecialty.Subject;
            var classInfo = assignTrainee.ClassSubject.Class;
            if (subject == null || classInfo == null)
                throw new Exception("Subject or Class not found.");

            // Get the course information for the class
            var course = await _courseRepository.GetCourseByClassIdAsync(classInfo.ClassId);
            if (course == null)
                throw new Exception("Course not found for the class.");

            var instructorAssign = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                ia => ia.SubjectId == existing.TraineeAssign.ClassSubject.SubjectSpecialty.SubjectId && ia.InstructorId == gradedByUserId);
            if (instructorAssign == null)
                throw new InvalidOperationException("User is not authorized to grade this subject.");

            if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected ||
                course.Progress == Progress.NotYet || course.Progress == Progress.Completed)
                throw new InvalidOperationException("Course isn't suitable to update grade.");

            var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                c => c.CourseId == course.CourseId && c.Status == CertificateStatus.Active);
            var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
            if (traineeWithCerts.Contains(assignTrainee.TraineeId))
                throw new InvalidOperationException($"Cannot update grade for TraineeAssignID '{existing.TraineeAssignID}' because a certificate has already been issued and active.");

            await ValidateGradeDto(dto);

            _mapper.Map(dto, existing);
            
            existing.TotalScore = CalculateTotalScore(existing);
            var passScore = subject.PassingScore;

            // Update grade status based on total score
            existing.gradeStatus = existing.TotalScore >= passScore ? GradeStatus.Pass : GradeStatus.Fail;

            existing.UpdateDate = DateTime.Now;
            existing.GradedByInstructorId = gradedByUserId;
            await _unitOfWork.GradeRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            if (existing.gradeStatus == GradeStatus.Pass)
            {
                if (course.CourseLevel == CourseLevel.Initial)
                {
                    if (!traineeWithCerts.Contains(assignTrainee.TraineeId))
                    {
                        await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, existing.GradedByInstructorId);
                        var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                        await _decisionService.CreateDecisionForCourseAsync(decisionRequest, existing.GradedByInstructorId);
                    }
                }
                else if (course.CourseLevel == CourseLevel.Recurrent)
                {
                    await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, existing.GradedByInstructorId);
                    var existingDecision = await _unitOfWork.DecisionRepository.GetAsync(
                        d => d.Certificate.CourseId == course.CourseId);
                    if (existingDecision == null)
                    {
                        var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                        await _decisionService.CreateDecisionForCourseAsync(decisionRequest, existing.GradedByInstructorId);
                    }
                }
                else if (course.CourseLevel == CourseLevel.Relearn)
                {
                    await HandleRelearnGradeAndCertificateAsync(existing, course, assignTrainee.TraineeId);
                }
            }
            if (existing.gradeStatus == GradeStatus.Fail)
            {
                var certificate = await _unitOfWork.CertificateRepository.GetFirstOrDefaultAsync(
                    c => c.UserId == assignTrainee.TraineeId && c.CourseId == course.CourseId);
                if (certificate != null)
                {
                    var requestDto = new RequestDTO
                    {
                        RequestType = RequestType.Revoke,
                        RequestEntityId = certificate.CertificateId,
                        Description = $"Request to revoke certificate for TraineeAssignID '{existing.TraineeAssignID}' in Course '{course.CourseId}' due to failed grade status.",
                        Notes = "Automated revoke request due to grade failure."
                    };
                    await _requestService.Value.CreateRequestAsync(requestDto, gradedByUserId);
                }
            }

            await _progressTrackingService.CheckAndUpdateClassSubjectStatus(assignTrainee.ClassSubjectId);

            return true;
        }
        #endregion

        #region Delete Grade
        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await _unitOfWork.GradeRepository.GetAsync(
                g => g.GradeId == id,
                g => g.TraineeAssign,
                g => g.TraineeAssign.ClassSubject,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty,
                g => g.TraineeAssign.ClassSubject.Class,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty.Subject);
            if (existing == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            var assignTrainee = existing.TraineeAssign;
            var classInfo = assignTrainee.ClassSubject.Class;

            // Get the course information for the class
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(classInfo.CourseId);
            if (course == null)
                throw new Exception("Course not found for the class.");

            var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                c => c.CourseId == course.CourseId);
            var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
            if (traineeWithCerts.Any())
                throw new InvalidOperationException($"Cannot delete grade for TraineeAssignID '{existing.TraineeAssignID}' because a certificate has already been issued.");

            // First update the trainee assignment to remove the reference to this grade
            //assignTrainee.GradeId = null;
            await _unitOfWork.TraineeAssignRepository.UpdateAsync(assignTrainee);

            // Then delete the grade
            await _unitOfWork.GradeRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion

        #region Get Grades By SubjectId
        public async Task<List<GradeModel>> GetGradesBySubjectIdAsync(string subjectId, string userId)
        {
            // Check if the user is an authorized instructor for the subject
            var instructorAssignments = await _unitOfWork.InstructorAssignmentRepository.FindAsync(
                ia => ia.SubjectId == subjectId &&
                      ia.InstructorId == userId &&
                      ia.RequestStatus == RequestStatus.Approved);
            if (!instructorAssignments.Any())
                throw new InvalidOperationException("User is not authorized to view grades for this subject.");

            // Use a modified approach to fetch grades for the subject
            // First, get all trainee assignments for the subject using the class subject table
            var traineeAssignments = await _unitOfWork.TraineeAssignRepository.FindAsync(
                ta => ta.ClassSubject.SubjectSpecialty.SubjectId == subjectId,
                ta => ta.ClassSubject,
                ta => ta.ClassSubject.SubjectSpecialty);

            // Then get grades for these trainee assignments
            var traineeAssignIds = traineeAssignments.Select(ta => ta.TraineeAssignId).ToList();
            var grades = await _unitOfWork.GradeRepository.FindAsync(
                g => traineeAssignIds.Contains(g.TraineeAssignID),
                g => g.TraineeAssign,
                g => g.TraineeAssign.Trainee,
                g => g.TraineeAssign.ClassSubject,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty);

            return _mapper.Map<List<GradeModel>>(grades);
        }
        #endregion

        #region Get Grades By UserId
        public async Task<List<GradeModel>> GetGradesByUserIdAsync(string userId)
        {
            var grades = await _unitOfWork.GradeRepository.FindIncludeAsync(
                g => g.TraineeAssign.TraineeId == userId,
                include => include.TraineeAssign,
                include => include.TraineeAssign.Trainee,
                include => include.TraineeAssign.ClassSubject,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty.Subject);

            return _mapper.Map<List<GradeModel>>(grades);
        }
        #endregion

        #region Get Grade By Status
        public async Task<List<GradeModel>> GetGradesByStatusAsync(GradeStatus status)
        {
            var grades = await _unitOfWork.GradeRepository.FindAsync(
                g => g.gradeStatus == status,
                g => g.TraineeAssign,
                g => g.TraineeAssign.Trainee,
                g => g.TraineeAssign.ClassSubject,
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty.Subject);

            return _mapper.Map<List<GradeModel>>(grades);
        }
        #endregion

        #region Get Grade By InstructorId
        public async Task<List<GradeModel>> GetGradesByInstructorIdAsync(string instructorId)
        {
            if (string.IsNullOrEmpty(instructorId))
                throw new ArgumentException("Instructor ID is required.");

            var instructor = await _unitOfWork.UserRepository.GetByIdAsync(instructorId);
            if (instructor == null)
                throw new KeyNotFoundException($"Instructor with ID '{instructorId}' not found.");

            if (instructor.RoleId != 5 && instructor.RoleId != 3)
                throw new UnauthorizedAccessException("Only instructors or admins can access trainee grades.");

            var instructorAssignments = await _unitOfWork.InstructorAssignmentRepository.FindAsync(
                a => a.InstructorId == instructorId && a.RequestStatus == RequestStatus.Approved);
            if (!instructorAssignments.Any())
                return new List<GradeModel>();

            var subjectIds = instructorAssignments.Select(a => a.SubjectId).Distinct().ToList();

            var grades = await _unitOfWork.GradeRepository.FindIncludeAsync(
                g => g.TraineeAssign.ClassSubject.SubjectSpecialty != null &&
                     subjectIds.Contains(g.TraineeAssign.ClassSubject.SubjectSpecialty.SubjectId),
                include => include.TraineeAssign,
                include => include.TraineeAssign.Trainee,
                include => include.TraineeAssign.ClassSubject,
                include => include.TraineeAssign.ClassSubject.SubjectSpecialty);

            return _mapper.Map<List<GradeModel>>(grades);
        }
        #endregion

        #region Import Grade
        public async Task<ImportResult> ImportGradesFromExcelAsync(Stream fileStream, string importedByUserId)
        {
            var result = new ImportResult
            {
                TotalRecords = 0,
                SuccessCount = 0,
                FailedCount = 0,
                Errors = new List<string>(),
                Warnings = new List<string>()
            };

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets["GradeImport"];
                    if (worksheet == null)
                    {
                        result.Errors.Add("Missing 'GradeImport' sheet.");
                        return result;
                    }

                    var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == importedByUserId);
                    if (user == null)
                    {
                        result.Errors.Add("Unauthorized.");
                        return result;
                    }

                    string classSubjectId = worksheet.Cells[1, 2].GetValue<string>();
                    if (string.IsNullOrEmpty(classSubjectId))
                    {
                        result.Errors.Add("ClassSubjectId is missing in cell B1.");
                        return result;
                    }

                    var classSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(classSubjectId);

                    // Load the necessary related entities after retrieving the class subject
                    if (classSubject != null)
                    {
                        await _unitOfWork.Context.Entry(classSubject)
                            .Reference(cs => cs.SubjectSpecialty).LoadAsync();

                        if (classSubject.SubjectSpecialty != null)
                        {
                            await _unitOfWork.Context.Entry(classSubject.SubjectSpecialty)
                                .Reference(ss => ss.Subject).LoadAsync();
                        }

                        await _unitOfWork.Context.Entry(classSubject)
                            .Reference(cs => cs.Class).LoadAsync();
                    }

                    if (classSubject == null)
                    {
                        result.Errors.Add($"ClassSubject with ID '{classSubjectId}' not found.");
                        return result;
                    }

                    var subject = classSubject.SubjectSpecialty.Subject;
                    if (subject == null)
                    {
                        result.Errors.Add($"Subject not found for ClassSubjectId '{classSubjectId}'.");
                        return result;
                    }

                    // Find SubjectSpecialty records for the user's specialty
                    var subjectSpecialties = await _unitOfWork.SubjectSpecialtyRepository.FindAsync(
                        ss => ss.SubjectId == subject.SubjectId && ss.SpecialtyId == user.SpecialtyId);
                    if (!subjectSpecialties.Any())
                    {
                        result.Errors.Add($"No specialty association found for Subject '{subject.SubjectName}' and user's specialty.");
                        return result;
                    }

                    var instructorAssign = await _unitOfWork.InstructorAssignmentRepository.FirstOrDefaultAsync(
                        ia => ia.SubjectId == subject.SubjectId && ia.InstructorId == importedByUserId);
                    if (instructorAssign == null)
                    {
                        result.Errors.Add("User is not authorized to grade this subject.");
                        return result;
                    }

                    // Get course information from classSubject.Class
                    var classInfo = classSubject.Class;
                    if (classInfo == null)
                    {
                        result.Errors.Add($"Class not found for ClassSubjectId '{classSubjectId}'.");
                        return result;
                    }

                    var course = await _unitOfWork.CourseRepository.GetByIdAsync(classInfo.CourseId);
                    if (course == null)
                    {
                        result.Errors.Add($"Course with ID '{classInfo.CourseId}' not found.");
                        return result;
                    }

                    if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected ||
                         course.Progress == Progress.Completed)
                    {
                        result.Errors.Add("Course isn't suitable to create grades.");
                        return result;
                    }

                    var schedule = await _trainingScheduleRepository.GetSchedulesByClassSubjectIdAsync(classSubject.ClassSubjectId);
                    if (schedule == null)
                    {
                        result.Errors.Add("ClassSubject does not have any training schedule.");
                        return result;
                    }

                    var existingGrades = await _unitOfWork.GradeRepository.GetAllAsync(g => g.TraineeAssign);
                    var existingGradeKeys = existingGrades
                        .Select(g => g.TraineeAssignID)
                        .ToHashSet();

                    // Get all trainees with their user information
                    var existingTraineeAssigns = await _unitOfWork.TraineeAssignRepository.FindAsync(
                        t => true, // Use a predicate that returns a boolean
                        t => t.ClassSubject,
                        t => t.ClassSubject.Class,
                        t => t.ClassSubject.SubjectSpecialty,
                        t => t.ClassSubject.SubjectSpecialty.Subject,
                        t => t.Trainee);

                    var assignMap = existingTraineeAssigns
                        .Where(a => a.ClassSubject.ClassSubjectId == classSubjectId)
                        .ToDictionary(a => a.TraineeId, a => (a.TraineeAssignId, a.TraineeId, a.Trainee));
                    var updateGrades = new List<Grade>();
                    int rowCount = worksheet.Dimension.Rows;
                    result.TotalRecords = rowCount - 2;

                    for (int row = 3; row <= rowCount; row++)
                    {
                        string traineeId = worksheet.Cells[row, 1].GetValue<string>();
                        if (string.IsNullOrWhiteSpace(traineeId))
                        {
                            result.Errors.Add($"Row {row}: TraineeId is missing.");
                            result.FailedCount++;
                            continue;
                        }

                        if (!assignMap.TryGetValue(traineeId, out var assignData))
                        {
                            result.Errors.Add($"Row {row}: No TraineeAssign found for TraineeId '{traineeId}' in Subject '{subject.SubjectName}'.");
                            result.FailedCount++;
                            continue;
                        }

                        string assignId = assignData.TraineeAssignId;
                        string traineeUserId = assignData.TraineeId;
                        User traineeUser = assignData.Trainee;

                        if (traineeUser.SpecialtyId != classSubject.SubjectSpecialty.SpecialtyId)
                        {
                            result.Warnings.Add($"Row {row}: Trainee '{traineeId}' specialty ({traineeUser.SpecialtyId}) doesn't match subject specialty ({classSubject.SubjectSpecialty.SpecialtyId}).");
                        }

                        // Check if the trainee already has a grade for this assignment
                        var existingGradeForTrainee = await _unitOfWork.GradeRepository.GetFirstOrDefaultAsync(g => g.TraineeAssignID == assignId);
                        
                        bool validScores = true;

                        double TryParseScore(int col)
                        {
                            var val = worksheet.Cells[row, col].GetValue<string>();
                            if (double.TryParse(val, out double score) && score >= 0 && score <= 10)
                                return score;

                            validScores = false;
                            return 0;
                        }

                        double participant = TryParseScore(2);
                        double assignment = TryParseScore(3);
                        double finalExam = TryParseScore(4);
                        double finalResit = TryParseScore(5);
                        string remarks = worksheet.Cells[row, 6].GetValue<string>() ?? "";

                        if (!validScores)
                        {
                            result.Errors.Add($"Row {row}: One or more scores are invalid. Must be between 0–10.");
                            result.FailedCount++;
                            continue;
                        }

                        var gradeDto = new GradeDTO
                        {
                            TraineeAssignID = assignId,
                            ParticipantScore = participant,
                            AssignmentScore = assignment,
                            FinalExamScore = finalExam,
                            FinalResitScore = finalResit,
                            Remarks = remarks
                        };

                        if (existingGradeForTrainee != null)
                        {
                            // Update the grade
                            try
                            {
                                await UpdateAsync(existingGradeForTrainee.GradeId, gradeDto, importedByUserId);
                                result.SuccessCount++;
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Row {row}: Failed to update grade for TraineeAssignId '{assignId}': {ex.Message}");
                                result.FailedCount++;
                            }
                        }
                        else
                        {
                            // Create new grade (optional, or skip if only updating)
                            // ... (existing creation logic)
                        }
                    }

                    if (updateGrades.Any())
                    {
                        // Update all the grades
                        foreach (var grade in updateGrades)
                        {
                            await _unitOfWork.GradeRepository.UpdateAsync(grade);
                        }
                        await _unitOfWork.SaveChangesAsync();

                        try
                        {
                            var passingGrades = updateGrades.Where(g => g.gradeStatus == GradeStatus.Pass).ToList();
                            if (passingGrades.Any())
                            {
                                // Process all grades first
                                foreach (var grade in passingGrades)
                                {
                                    var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(grade.TraineeAssignID);
                                    if (traineeAssign == null) continue;

                                    // Check if all subjects in this course are completed for this trainee
                                    await CheckAndProcessCourseCompletion(course.CourseId, traineeAssign.TraineeId, importedByUserId);
                                }

                                result.AdditionalInfo = $"Successfully processed {passingGrades.Count} passing grades. Certificates will be generated after all subjects in a course are completed.";
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Warnings.Add($"Grades were updated successfully, but certificate/decision generation failed: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"An error occurred while importing grades: {ex.Message}");
            }

            return result;
        }
        #endregion

        #region Helper Methods
        private double CalculateTotalScore(Grade grade)
        {
            double participant = grade.ParticipantScore * 0.1;
            double assignment = grade.AssignmentScore * 0.3;
            double finalScore = (grade.FinalResitScore > 0) ? grade.FinalResitScore.Value : grade.FinalExamScore;
            double final = finalScore * 0.6;
            return participant + assignment + final;
        }

        private async Task ValidateGradeDto(GradeDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Grade data is required.");

            if (string.IsNullOrEmpty(dto.TraineeAssignID))
                throw new ArgumentException("TraineeAssignID is required.");

            double[] scores =
            {
                dto.ParticipantScore, dto.AssignmentScore,
                dto.FinalExamScore, dto.FinalResitScore ?? 0
            };

            foreach (var score in scores)
            {
                if (score < 0 || score > 10)
                    throw new ArgumentOutOfRangeException(nameof(score), "Scores must be between 0 and 10.");
            }

            var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetAsync(
                t => t.TraineeAssignId == dto.TraineeAssignID,
                t => t.ClassSubject, 
                t => t.ClassSubject.SubjectSpecialty,  // Include the SubjectSpecialty navigation property
                t => t.ClassSubject.SubjectSpecialty.Subject);
            if (traineeAssign == null)
                throw new InvalidOperationException("Trainee assignment not found.");
            if(traineeAssign.RequestStatus != RequestStatus.Approved)
            {
                throw new InvalidOperationException("Trainee assignment not approved yet.");
            }
            if (traineeAssign.ClassSubject?.SubjectSpecialty.SubjectId == null)
                throw new InvalidOperationException("Subject not found for this trainee assignment.");
        }

        private async Task HandleRelearnGradeAndCertificateAsync(Grade grade, Course relearnCourse, string traineeId)
        {
            // 1. Take original information from RelatedCourseId
            var originalCourse = await _unitOfWork.CourseRepository.GetByIdAsync(relearnCourse.RelatedCourseId);
            if (originalCourse == null)
                throw new InvalidOperationException("Original course not found for relearn processing");

            // 2. Get all ClassSubjects linked to the original course
            var originalClassSubjects = await _unitOfWork.ClassSubjectRepository.FindAsync(
                cs => cs.ClassId == originalCourse.CourseId);

            // 3. Get all failed subjects in the original course
            var failedSubjects = new List<string>();
            foreach (var cs in originalClassSubjects)
            {
                var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetAsync(
                    ta => ta.ClassSubjectId == cs.ClassSubjectId && ta.TraineeId == traineeId);

                if (traineeAssign != null)
                {
                    var gradeForSubject = await _unitOfWork.GradeRepository.GetAsync(
                        g => g.TraineeAssignID == traineeAssign.TraineeAssignId);

                    if (gradeForSubject != null && gradeForSubject.gradeStatus == GradeStatus.Fail)
                        failedSubjects.Add(cs.SubjectSpecialty.SubjectId);
                }
            }

            // 4. No need to process if there are no failed subjects
            if (failedSubjects.Count == 0)
                return;

            // 5. Get all ClassSubjects linked to the relearn course
            var relearnClassSubjects = await _unitOfWork.ClassSubjectRepository.FindAsync(
                cs => cs.ClassId == relearnCourse.CourseId);

            // 6. Check if student is assigned to the appropriate relearn course
            var traineeAssignment = await _unitOfWork.TraineeAssignRepository.GetAsync(
                ta => ta.TraineeAssignId == grade.TraineeAssignID);

            if (traineeAssignment == null)
                throw new InvalidOperationException("Student assignment not found");

            var relearnClassSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(
                traineeAssignment.ClassSubjectId);

            if (relearnClassSubject == null)
                throw new InvalidOperationException("ClassSubject information not found");

            // 7. Get the list of subjects in the relearn course
            var relearnSubjectIds = relearnClassSubjects
                .Select(cs => cs.SubjectSpecialty.Subject)
                .ToList();

            // 8. Check if all failed subjects are included in the relearn course
            bool allFailedSubjectsIncluded = failedSubjects.All(s => relearnSubjectIds.Any(rs => rs.SubjectId == s)); 
            if (!allFailedSubjectsIncluded)
            {
                throw new InvalidOperationException("Relearn course does not contain all failed subjects from the original course");
            }

            // 9. Check if all failed subjects are passed in the relearn course
            bool allRelearnSubjectsPassed = true;
            foreach (var subjectId in failedSubjects)
            {
                var classSubjectForSubject = relearnClassSubjects.FirstOrDefault(cs =>
                    cs.SubjectSpecialty.SubjectId == subjectId);

                if (classSubjectForSubject != null)
                {
                    var traineeAssignForSubject = await _unitOfWork.TraineeAssignRepository.GetAsync(
                        ta => ta.ClassSubjectId == classSubjectForSubject.ClassSubjectId && ta.TraineeId == traineeId);

                    if (traineeAssignForSubject != null)
                    {
                        var gradeForSubject = await _unitOfWork.GradeRepository.GetAsync(
                            g => g.TraineeAssignID == traineeAssignForSubject.TraineeAssignId);

                        if (gradeForSubject == null || gradeForSubject.gradeStatus != GradeStatus.Pass)
                        {
                            allRelearnSubjectsPassed = false;
                            break;
                        }
                    }
                    else
                    {
                        allRelearnSubjectsPassed = false;
                        break;
                    }
                }
                else
                {
                    allRelearnSubjectsPassed = false;
                    break;
                }
            }

            // 10. No processing in this method - aftermath handling is done in calling methods
        }

        // Tìm khóa không phải relearn gần nhất, không phải lúc nào cũng là khóa "gốc"
        private async Task<Course> FindFirstNonRelearnCourse(Course course)
        {
            if (course == null)
                throw new InvalidOperationException("Course not found");

            // Nếu không phải khóa học Relearn, đây là khóa học cần tìm
            if (course.CourseLevel != CourseLevel.Relearn)
                return course;

            // Tìm khóa học liên quan trực tiếp
            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(course.RelatedCourseId);
            if (relatedCourse == null)
                return null;

            // Kiểm tra xem khóa học liên quan có phải Relearn không
            if (relatedCourse.CourseLevel != CourseLevel.Relearn)
                return relatedCourse; // Trả về ngay nếu không phải Relearn

            // Nếu vẫn là Relearn, tiếp tục tìm
            return await FindFirstNonRelearnCourse(relatedCourse);
        }

        // Add new helper method to check course completion and process certificates
        private async Task CheckAndProcessCourseCompletion(string courseId, string traineeId, string processedByUserId)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new KeyNotFoundException($"Course with ID '{courseId}' not found.");

            // Get all ClassSubjects linked to this course
            var classSubjects = await _unitOfWork.ClassSubjectRepository.FindAsync(
                cs => cs.ClassId == courseId,
                cs => cs.SubjectSpecialty.Subject);

            // Get all trainee assignments for this trainee in the course
            var traineeAssignments = await _unitOfWork.TraineeAssignRepository.FindAsync(
                ta => classSubjects.Select(cs => cs.ClassSubjectId).Contains(ta.ClassSubjectId) && ta.TraineeId == traineeId);

            // Skip processing if there are no trainee assignments
            if (!traineeAssignments.Any())
                return;

            // Get all grades for the assignments
            var grades = await _unitOfWork.GradeRepository.FindAsync(
                g => traineeAssignments.Select(ta => ta.TraineeAssignId).Contains(g.TraineeAssignID));

            // Create a set of subjects that have passing grades
            var subjectsWithPassingGrades = new HashSet<string>();
            foreach (var grade in grades.Where(g => g.gradeStatus == GradeStatus.Pass))
            {
                var traineeAssign = traineeAssignments.FirstOrDefault(ta => ta.TraineeAssignId == grade.TraineeAssignID);
                if (traineeAssign != null)
                {
                    var classSubject = classSubjects.FirstOrDefault(cs => cs.ClassSubjectId == traineeAssign.ClassSubjectId);
                    if (classSubject != null)
                    {
                        subjectsWithPassingGrades.Add(classSubject.SubjectSpecialty.SubjectId);
                    }
                }
            }

            // Get all required subjects for the course
            var allRequiredSubjects = await GetAllRequiredSubjects(course);

            // Check if all required subjects have passing grades
            var allRequiredSubjectsPassed = allRequiredSubjects.All(subjectsWithPassingGrades.Contains);

            if (allRequiredSubjectsPassed)
            {
                // Check if there are any active certificates for this course and trainee
                var existingCertificate = await _unitOfWork.CertificateRepository.GetFirstOrDefaultAsync(
                    c => c.CourseId == courseId && c.UserId == traineeId && c.Status == CertificateStatus.Active);

                if (existingCertificate == null)
                {
                    // Generate certificate
                    if (course.CourseLevel == CourseLevel.Initial || course.CourseLevel == CourseLevel.Recurrent)
                    {
                        await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(courseId, processedByUserId);

                        // Create decision document if needed
                        var existingDecision = await _unitOfWork.DecisionRepository.GetFirstOrDefaultAsync(
                            d => d.Certificate.CourseId == courseId);
                        if (existingDecision == null)
                        {
                            var decisionRequest = new CreateDecisionDTO { CourseId = courseId };
                            await _decisionService.CreateDecisionForCourseAsync(decisionRequest, processedByUserId);
                        }
                    }
                    else if (course.CourseLevel == CourseLevel.Relearn)
                    {
                        // For relearn courses, we need to find the root course
                        var rootCourse = await FindRootCourse(course);

                        if (await IsRootCoursePassed(rootCourse, traineeId))
                        {
                            // If already passed, perhaps just update the certificate extension date
                            await UpdateExistingCertificate(rootCourse, traineeId, processedByUserId);
                            return;
                        }

                        // Check if all required subjects are passed through the chain
                        var allRequiredPassedThroughChain = await ValidateAllRequiredSubjectsPassedThroughRelearnChain(rootCourse, traineeId);

                        if (allRequiredPassedThroughChain)
                        {
                            // Update certificate for the root course
                            await UpdateExistingCertificate(rootCourse, traineeId, processedByUserId);
                        }
                    }
                }

                // Update course progress
                if (course.Progress != Progress.Completed)
                {
                    course.Progress = Progress.Completed;
                    await _unitOfWork.CourseRepository.UpdateAsync(course);
                }
            }
        }

        private async Task<List<string>> GetAllRequiredSubjects(Course course)
        {
            // Get all ClassSubjects linked to this course
            var classSubjects = await _unitOfWork.ClassSubjectRepository.FindAsync(
                cs => cs.ClassId == course.CourseId,
                cs => cs.SubjectSpecialty.Subject);

            return classSubjects.Select(cs => cs.SubjectSpecialty.SubjectId).ToList();
        }

        private async Task<List<Grade>> CollectBestGradesForAllSubjects(Course rootCourse, string traineeId)
        {
            // Get all related courses in the chain
            var allRelatedCourses = await GetAllRelatedCourses(rootCourse);
            var courseIds = allRelatedCourses.Select(c => c.CourseId).ToList();

            // Get all ClassSubjects from these courses
            var classSubjects = await _unitOfWork.ClassSubjectRepository.FindAsync(
                cs => courseIds.Contains(cs.ClassId),
                cs => cs.SubjectSpecialty.Subject);

            // Group ClassSubjects by SubjectId
            var subjectGroups = classSubjects.GroupBy(cs => cs.SubjectSpecialty.SubjectId).ToDictionary(g => g.Key, g => g.ToList());

            // Get all trainee assignments for this trainee across all courses
            var traineeAssignments = await _unitOfWork.TraineeAssignRepository.FindAsync(
                ta => classSubjects.Select(cs => cs.ClassSubjectId).Contains(ta.ClassSubjectId) && ta.TraineeId == traineeId);

            if (!traineeAssignments.Any())
                return new List<Grade>();

            // Get all grades for these assignments
            var grades = await _unitOfWork.GradeRepository.FindAsync(
                g => traineeAssignments.Select(ta => ta.TraineeAssignId).Contains(g.TraineeAssignID),
                g => g.TraineeAssign);

            // Map grades to their subjects
            var gradesBySubject = new Dictionary<string, List<Grade>>();
            foreach (var grade in grades)
            {
                var traineeAssign = traineeAssignments.FirstOrDefault(ta => ta.TraineeAssignId == grade.TraineeAssignID);
                if (traineeAssign != null)
                {
                    var classSubject = classSubjects.FirstOrDefault(cs => cs.ClassSubjectId == traineeAssign.ClassSubjectId);
                    if (classSubject != null)
                    {
                        var subjectId = classSubject.SubjectSpecialty.SubjectId;
                        if (!gradesBySubject.ContainsKey(subjectId))
                        {
                            gradesBySubject[subjectId] = new List<Grade>();
                        }
                        gradesBySubject[subjectId].Add(grade);
                    }
                }
            }

            // Get the best grade for each subject
            var bestGrades = new List<Grade>();
            foreach (var entry in gradesBySubject)
            {
                var bestGrade = entry.Value.OrderByDescending(g => g.TotalScore).FirstOrDefault();
                if (bestGrade != null)
                {
                    bestGrades.Add(bestGrade);
                }
            }

            return bestGrades;
        }

        private async Task<List<Course>> GetAllRelatedCourses(Course rootCourse)
        {
            var result = new List<Course> { rootCourse };

            // Tìm tất cả khóa Relearn có RelatedCourseId là rootCourse.CourseId
            var directRelearns = await _unitOfWork.CourseRepository.FindAsync(
                c => c.RelatedCourseId == rootCourse.CourseId && c.CourseLevel == CourseLevel.Relearn);

            result.AddRange(directRelearns);

            // Đệ quy tìm các khóa Relearn của các khóa Relearn
            foreach (var relearn in directRelearns)
            {
                var subRelearns = await GetAllRelatedCourses(relearn);
                result.AddRange(subRelearns.Where(c => c.CourseId != relearn.CourseId)); // Tránh trùng lặp
            }

            return result.Distinct().ToList(); // Đảm bảo không trùng lặp
        }

        private async Task UpdateExistingCertificate(Course course, string traineeId, string processedByUserId)
        {
            if (course.CourseLevel == CourseLevel.Recurrent)
            {
                // Tìm khóa học gốc (Initial hoặc Professional)
                var rootCourse = await FindRootCourse(course);
                if (rootCourse == null)
                    return;

                // Nếu đã có chứng chỉ, không xử lý ở đây (để CertificateService lo)
                // Chỉ tạo decision và thông báo cho người dùng

                // Tạo decision cho course
                var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                await _decisionService.CreateDecisionForCourseAsync(decisionRequest, processedByUserId);
            }
            else
            {
                // Tạo decision cho course khác Recurrent
                var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                await _decisionService.CreateDecisionForCourseAsync(decisionRequest, processedByUserId);
            }
        }

        // Tìm khóa học gốc (Initial hoặc Professional)
        private async Task<Course> FindRootCourse(Course course)
        {
            if (course == null)
                return null;

            if (course.CourseLevel == CourseLevel.Initial || course.CourseLevel == CourseLevel.Professional)
                return course;

            if (string.IsNullOrEmpty(course.RelatedCourseId))
                return null;

            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(course.RelatedCourseId);
            if (relatedCourse == null)
                return null;

            return await FindRootCourse(relatedCourse);
        }

        private async Task<bool> ValidateAllRequiredSubjectsPassedThroughRelearnChain(Course originalCourse, string traineeId)
        {
            // Lấy tất cả môn học của khóa gốc
            var originalClassSubjects = await _unitOfWork.ClassSubjectRepository.FindAsync(
                cs => cs.ClassId == originalCourse.CourseId);

            var requiredSubjectIds = originalClassSubjects.Select(cs => cs.SubjectSpecialty.SubjectId).Distinct().ToList();
            var passedSubjectIds = new HashSet<string>();

            // Tìm tất cả khóa học relearn liên quan
            var relatedCourses = new List<Course> { originalCourse };
            var currentCourse = originalCourse;

            // Tìm các khóa relearn liên quan đến khóa gốc
            var relearnCourses = await _unitOfWork.CourseRepository.FindAsync(
                c => c.RelatedCourseId == originalCourse.CourseId && c.CourseLevel == CourseLevel.Relearn);
            relatedCourses.AddRange(relearnCourses);

            // Tìm các khóa relearn liên quan đến khóa relearn
            foreach (var relearn in relearnCourses)
            {
                var subRelearnCourses = await _unitOfWork.CourseRepository.FindAsync(
                    c => c.RelatedCourseId == relearn.CourseId && c.CourseLevel == CourseLevel.Relearn);
                relatedCourses.AddRange(subRelearnCourses);
            }

            // Kiểm tra điểm của tất cả các môn học trong tất cả các khóa liên quan
            foreach (var course in relatedCourses)
            {
                var courseClassSubjects = await _unitOfWork.ClassSubjectRepository.FindAsync(
                    cs => cs.ClassId == course.CourseId);

                foreach (var cs in courseClassSubjects)
                {
                    if (requiredSubjectIds.Contains(cs.SubjectSpecialty.SubjectId))
                    {
                        var traineeAssigns = await _unitOfWork.TraineeAssignRepository.FindAsync(
                            ta => ta.ClassSubjectId == cs.ClassSubjectId && ta.TraineeId == traineeId);

                        foreach (var ta in traineeAssigns)
                        {
                            var grade = await _unitOfWork.GradeRepository.GetAsync(
                                g => g.TraineeAssignID == ta.TraineeAssignId && g.gradeStatus == GradeStatus.Pass);

                            if (grade != null)
                            {
                                passedSubjectIds.Add(cs.SubjectSpecialty.SubjectId);
                            }
                        }
                    }
                }
            }

            // Kiểm tra xem tất cả môn học yêu cầu đã được pass chưa
            return requiredSubjectIds.All(id => passedSubjectIds.Contains(id));
        }

        // Thêm phương thức mới để kiểm tra xem root course đã pass chưa
        private async Task<bool> IsRootCoursePassed(Course rootCourse, string traineeId)
        {
            // Kiểm tra xem trainee đã có chứng chỉ active cho khóa học này chưa
            var existingCertificate = await _unitOfWork.CertificateRepository.GetFirstOrDefaultAsync(
                c => c.UserId == traineeId &&
                     c.CourseId == rootCourse.CourseId &&
                     c.Status == CertificateStatus.Active);

            return existingCertificate != null;
        }
        #endregion
    }
}
