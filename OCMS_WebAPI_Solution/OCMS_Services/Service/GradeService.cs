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
using System.Linq;
using System.Text;
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
        private readonly IProgressTrackingService _progressTrackingService;
        private readonly IRequestService _requestService;

        public GradeService(UnitOfWork unitOfWork, IMapper mapper, ICertificateService certificateService,
            ITrainingScheduleRepository trainingScheduleRepository, IDecisionService decisionService,
            IProgressTrackingService progressTrackingService, IRequestService requestService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _certificateService = certificateService;
            _trainingScheduleRepository = trainingScheduleRepository;
            _decisionService = decisionService;
            _progressTrackingService = progressTrackingService;
            _requestService = requestService;
        }

        #region Get All Grades
        public async Task<IEnumerable<GradeModel>> GetAllAsync()
        {
            var grades = await _unitOfWork.GradeRepository.GetAllAsync(
                g => g.TraineeAssign,
                g => g.TraineeAssign.CourseSubjectSpecialty,
                g => g.TraineeAssign.Trainee);
            var gradeModels = new List<GradeModel>();

            foreach (var grade in grades)
            {
                var gradeModel = _mapper.Map<GradeModel>(grade);
                gradeModel.Fullname = grade.TraineeAssign?.Trainee?.FullName;
                gradeModels.Add(gradeModel);
            }

            return gradeModels;
        }
        #endregion

        #region Get Grade By Id
        public async Task<GradeModel> GetByIdAsync(string id)
        {
            var grade = await _unitOfWork.GradeRepository.GetAsync(
                g => g.GradeId == id,
                g => g.TraineeAssign,
                g => g.TraineeAssign.CourseSubjectSpecialty,
                g => g.TraineeAssign.Trainee);
            if (grade == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            var gradeModel = _mapper.Map<GradeModel>(grade);
            gradeModel.Fullname = grade.TraineeAssign?.Trainee?.FullName;
            return gradeModel;
        }
        #endregion

        #region Create Grade
        public async Task<string> CreateAsync(GradeDTO dto, string gradedByUserId)
        {
            var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetAsync(
                t => t.TraineeAssignId == dto.TraineeAssignID,
                t => t.CourseSubjectSpecialty,
                t => t.CourseSubjectSpecialty.Subject,
                t => t.CourseSubjectSpecialty.Course);
            if (traineeAssign == null)
                throw new KeyNotFoundException($"Trainee assignment with ID '{dto.TraineeAssignID}' not found.");

            var subject = traineeAssign.CourseSubjectSpecialty.Subject;
            var course = traineeAssign.CourseSubjectSpecialty.Course;
            if (subject == null || course == null)
                throw new Exception("Subject or Course not found.");

            var instructorAssign = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                ia => ia.CourseSubjectSpecialtyId == traineeAssign.CourseSubjectSpecialtyId && ia.InstructorId == gradedByUserId);
            if (instructorAssign == null)
                throw new InvalidOperationException("User is not authorized to grade this course subject specialty.");

            var schedule = await _trainingScheduleRepository.GetSchedulesByCourseSubjectIdAsync(traineeAssign.CourseSubjectSpecialtyId);
            if (schedule == null)
                throw new InvalidOperationException("CourseSubjectSpecialty does not have any training schedule.");

            if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected ||
                course.Progress == Progress.NotYet || course.Progress == Progress.Completed)
                throw new InvalidOperationException("Course isn't suitable to create grade.");

            var existingGrade = await _unitOfWork.GradeRepository.GetAsync(
                g => g.TraineeAssignID == dto.TraineeAssignID);
            if (existingGrade != null)
                throw new InvalidOperationException("Grade for this trainee assignment already exists.");

            await ValidateGradeDto(dto);

            var grade = _mapper.Map<Grade>(dto);
            grade.GradeId = $"G-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            grade.GradedByInstructorId = gradedByUserId;
            grade.EvaluationDate = DateTime.UtcNow;

            var passScore = subject.PassingScore;
            grade.TotalScore = CalculateTotalScore(grade);

            grade.gradeStatus = (grade.ParticipantScore == 0 || grade.AssignmentScore == 0)
                ? GradeStatus.Fail
                : grade.TotalScore >= passScore ? GradeStatus.Pass : GradeStatus.Fail;

            await _unitOfWork.GradeRepository.AddAsync(grade);
            await _unitOfWork.SaveChangesAsync();

            // If grade is passing, check course completion status
            if (grade.gradeStatus == GradeStatus.Pass)
            {
                await CheckAndProcessCourseCompletion(course.CourseId, traineeAssign.TraineeId, gradedByUserId);
            }

            await _unitOfWork.SaveChangesAsync();
            await _progressTrackingService.CheckAndUpdateCourseSubjectSpecialtyStatus(traineeAssign.CourseSubjectSpecialtyId);

            return grade.GradeId;
        }
        #endregion

        #region Update Grade
        public async Task<bool> UpdateAsync(string id, GradeDTO dto, string gradedByUserId)
        {
            var existing = await _unitOfWork.GradeRepository.GetAsync(
                g => g.GradeId == id,
                g => g.TraineeAssign,
                g => g.TraineeAssign.CourseSubjectSpecialty,
                g => g.TraineeAssign.CourseSubjectSpecialty.Subject,
                g => g.TraineeAssign.CourseSubjectSpecialty.Course);
            if (existing == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            var assignTrainee = await _unitOfWork.TraineeAssignRepository.GetAsync(
                t => t.TraineeAssignId == dto.TraineeAssignID,
                t => t.CourseSubjectSpecialty,
                t => t.CourseSubjectSpecialty.Subject,
                t => t.CourseSubjectSpecialty.Course);
            if (assignTrainee == null)
                throw new KeyNotFoundException($"Trainee assignment with ID '{dto.TraineeAssignID}' not found.");

            var subject = assignTrainee.CourseSubjectSpecialty.Subject;
            var course = assignTrainee.CourseSubjectSpecialty.Course;
            if (subject == null || course == null)
                throw new Exception("Subject or Course not found.");

            var instructorAssign = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                ia => ia.CourseSubjectSpecialtyId == assignTrainee.CourseSubjectSpecialtyId && ia.InstructorId == gradedByUserId);
            if (instructorAssign == null)
                throw new InvalidOperationException("User is not authorized to grade this course subject specialty.");

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

            existing.gradeStatus = (existing.ParticipantScore == 0 || existing.AssignmentScore == 0)
                ? GradeStatus.Fail
                : existing.TotalScore >= passScore ? GradeStatus.Pass : GradeStatus.Fail;

            existing.UpdateDate = DateTime.UtcNow;
            existing.GradedByInstructorId = gradedByUserId;
            await _unitOfWork.GradeRepository.UpdateAsync(existing);

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
                    await _requestService.CreateRequestAsync(requestDto, gradedByUserId);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _progressTrackingService.CheckAndUpdateCourseSubjectSpecialtyStatus(assignTrainee.CourseSubjectSpecialtyId);

            return true;
        }
        #endregion

        #region Delete Grade
        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await _unitOfWork.GradeRepository.GetAsync(
                g => g.GradeId == id,
                g => g.TraineeAssign,
                g => g.TraineeAssign.CourseSubjectSpecialty,
                g => g.TraineeAssign.CourseSubjectSpecialty.Course);
            if (existing == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            var assignTrainee = existing.TraineeAssign;
            var course = assignTrainee.CourseSubjectSpecialty.Course;

            var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                c => c.CourseId == course.CourseId);
            var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
            if (traineeWithCerts.Any())
                throw new InvalidOperationException($"Cannot delete grade for TraineeAssignID '{existing.TraineeAssignID}' because a certificate has already been issued.");

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
                ia => ia.CourseSubjectSpecialty.SubjectId == subjectId &&
                      ia.InstructorId == userId &&
                      ia.RequestStatus == RequestStatus.Approved,
                ia => ia.CourseSubjectSpecialty);
            if (!instructorAssignments.Any())
                throw new InvalidOperationException("User is not authorized to view grades for this subject.");

            // Fetch grades for the subject with authorization check integrated
            var grades = await _unitOfWork.GradeRepository.FindIncludeAsync(
                g => g.TraineeAssign.CourseSubjectSpecialty.SubjectId == subjectId,
                include => include.TraineeAssign,
                include => include.TraineeAssign.Trainee,
                include => include.TraineeAssign.CourseSubjectSpecialty);

            // Use AutoMapper to project the grades to GradeModel, including Fullname
            var gradeModels = _mapper.Map<List<GradeModel>>(grades);
            return gradeModels;
        }
        #endregion

        #region Get Grades By UserId
        public async Task<List<GradeModel>> GetGradesByUserIdAsync(string userId)
        {
            var grades = await _unitOfWork.GradeRepository.FindIncludeAsync(
                g => g.TraineeAssign.TraineeId == userId,
                include => include.TraineeAssign,
                include => include.TraineeAssign.Trainee);
            var gradeModels = _mapper.Map<List<GradeModel>>(grades);
            foreach (var gradeModel in gradeModels)
            {
                var grade = grades.First(g => g.GradeId == gradeModel.GradeId);
                gradeModel.Fullname = grade.TraineeAssign?.Trainee?.FullName;
            }
            return gradeModels;
        }
        #endregion

        #region Get Grade By Status
        public async Task<List<GradeModel>> GetGradesByStatusAsync(GradeStatus status)
        {
            var grades = await _unitOfWork.GradeRepository.FindAsync(
                g => g.gradeStatus == status,
                g => g.TraineeAssign,
                g => g.TraineeAssign.Trainee);
            var gradeModels = _mapper.Map<List<GradeModel>>(grades);
            foreach (var gradeModel in gradeModels)
            {
                var grade = grades.First(g => g.GradeId == gradeModel.GradeId);
                gradeModel.Fullname = grade.TraineeAssign?.Trainee?.FullName;
            }
            return gradeModels;
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
                a => a.InstructorId == instructorId && a.RequestStatus == RequestStatus.Approved,
                a => a.CourseSubjectSpecialty);
            if (!instructorAssignments.Any())
                return new List<GradeModel>();

            var cssIds = instructorAssignments.Select(a => a.CourseSubjectSpecialtyId).Distinct().ToList();

            var grades = await _unitOfWork.GradeRepository.FindIncludeAsync(
                g => g.TraineeAssign.CourseSubjectSpecialtyId != null && cssIds.Contains(g.TraineeAssign.CourseSubjectSpecialtyId),
                include => include.TraineeAssign,
                include => include.TraineeAssign.Trainee);

            var gradeModels = new List<GradeModel>();
            foreach (var grade in grades)
            {
                var gradeModel = _mapper.Map<GradeModel>(grade);
                gradeModel.Fullname = grade.TraineeAssign?.Trainee?.FullName;
                gradeModels.Add(gradeModel);
            }

            return gradeModels;
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

                    string subjectName = worksheet.Cells[1, 2].GetValue<string>();
                    if (string.IsNullOrEmpty(subjectName))
                    {
                        result.Errors.Add("Subject name is missing in cell B1.");
                        return result;
                    }

                    var subject = await _unitOfWork.SubjectRepository.FirstOrDefaultAsync(s => s.SubjectName == subjectName);
                    if (subject == null)
                    {
                        result.Errors.Add($"Subject '{subjectName}' not found.");
                        return result;
                    }

                    var cssList = await _unitOfWork.CourseSubjectSpecialtyRepository.FindAsync(
                        css => css.SubjectId == subject.SubjectId,
                        css => css.Course,
                        css => css.Instructors,
                        css => css.Specialty);  // Explicitly include Specialty
                    if (!cssList.Any())
                    {
                        result.Errors.Add($"No CourseSubjectSpecialty found for Subject '{subjectName}'.");
                        return result;
                    }

                    var instructorAssign = cssList
                        .SelectMany(css => css.Instructors)
                        .FirstOrDefault(ia => ia.InstructorId == importedByUserId);
                    if (instructorAssign == null)
                    {
                        result.Errors.Add("User is not authorized to grade this subject.");
                        return result;
                    }

                    var css = cssList.First();
                    var course = css.Course;
                    var courseSpecialty = await _unitOfWork.SpecialtyRepository.FirstOrDefaultAsync(s => s.SpecialtyId == user.SpecialtyId);

                    if (courseSpecialty == null)
                    {
                        result.Errors.Add($"Specialty not found for Subject '{subjectName}'.");
                        return result;
                    }
                    if (course == null)
                    {
                        result.Errors.Add($"Course not found for Subject '{subjectName}'.");
                        return result;
                    }

                    //if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected ||
                    //    course.Progress == Progress.NotYet || course.Progress == Progress.Completed)
                    //{
                    //    result.Errors.Add("Course isn't suitable to create grades.");
                    //    return result;
                    //}

                    var schedule = await _trainingScheduleRepository.GetSchedulesByCourseSubjectIdAsync(css.Id);
                    if (schedule == null)
                    {
                        result.Errors.Add("CourseSubjectSpecialty does not have any training schedule.");
                        return result;
                    }

                    var existingGrades = await _unitOfWork.GradeRepository.GetAllAsync(g => g.TraineeAssign);
                    var existingGradeKeys = existingGrades
                        .Select(g => g.TraineeAssignID)
                        .ToHashSet();

                    // Get all trainees with their user information to access specialty
                    var existingTraineeAssigns = await _unitOfWork.TraineeAssignRepository.GetAllAsync(
                        t => t.CourseSubjectSpecialty,
                        t => t.CourseSubjectSpecialty.Course,
                        t => t.CourseSubjectSpecialty.Specialty,
                        t => t.Trainee);  // Explicitly include Trainee to access User data

                    var assignMap = existingTraineeAssigns
                         .Where(a => a.CourseSubjectSpecialty.SubjectId == subject.SubjectId && a.CourseSubjectSpecialty.SpecialtyId == courseSpecialty.SpecialtyId)
                         .ToDictionary(a => a.TraineeId, a => (a.TraineeAssignId, a.TraineeId, a.Trainee));

                    var newGrades = new List<Grade>();
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

                        if (traineeUser.SpecialtyId != courseSpecialty.SpecialtyId)
                        {
                            result.Warnings.Add($"Row {row}: Trainee '{traineeId}' specialty ({traineeUser.SpecialtyId}) doesn't match course specialty ({courseSpecialty.SpecialtyId}).");
                        }

                        if (existingGradeKeys.Contains(assignId))
                        {
                            result.Errors.Add($"Row {row}: Grade already exists for TraineeAssignId '{assignId}'.");
                            result.FailedCount++;
                            continue;
                        }

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

                        var grade = new Grade
                        {
                            GradeId = $"G-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                            TraineeAssignID = assignId,
                            ParticipantScore = participant,
                            AssignmentScore = assignment,
                            FinalExamScore = finalExam,
                            FinalResitScore = finalResit,
                            GradedByInstructorId = importedByUserId,
                            Remarks = remarks,
                            EvaluationDate = DateTime.UtcNow
                        };

                        var passScore = subject.PassingScore;
                        grade.TotalScore = CalculateTotalScore(grade);

                        grade.gradeStatus = (grade.ParticipantScore == 0 || grade.AssignmentScore == 0)
                            ? GradeStatus.Fail
                            : grade.TotalScore >= passScore ? GradeStatus.Pass : GradeStatus.Fail;

                        newGrades.Add(grade);
                        result.SuccessCount++;
                    }

                    if (newGrades.Any())
                    {
                        await _unitOfWork.GradeRepository.AddRangeAsync(newGrades);
                        await _unitOfWork.SaveChangesAsync();

                        try
                        {
                            var passingGrades = newGrades.Where(g => g.gradeStatus == GradeStatus.Pass).ToList();
                            if (passingGrades.Any())
                            {
                                // Process all grades first
                                foreach (var grade in passingGrades)
                                {
                                    var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(grade.TraineeAssignID);
                                    if (traineeAssign == null) continue;

                                    // Check if all subjects in this course are completed for this trainee
                                    await CheckAndProcessCourseCompletion(traineeAssign.CourseSubjectSpecialty.CourseId, traineeAssign.TraineeId, importedByUserId);
                                }

                                result.AdditionalInfo = $"Successfully processed {passingGrades.Count} passing grades. Certificates will be generated after all subjects in a course are completed.";
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Warnings.Add($"Grades were imported successfully, but certificate/decision generation failed: {ex.Message}");
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
                t => t.CourseSubjectSpecialty,
                t => t.CourseSubjectSpecialty.Subject);
            if (traineeAssign == null)
                throw new InvalidOperationException("Trainee assignment not found.");

            if (traineeAssign.CourseSubjectSpecialty?.Subject == null)
                throw new InvalidOperationException("Subject not found for this trainee assignment.");
        }

        private async Task HandleRelearnGradeAndCertificateAsync(Grade grade, Course relearnCourse, string traineeId)
        {
            // 1. Take original information from RelatedCourseId
            var originalCourse = await _unitOfWork.CourseRepository.GetByIdAsync(relearnCourse.RelatedCourseId);
            if (originalCourse == null)
                throw new InvalidOperationException("Original course not found for relearn processing");
            
            // 2. Get all CourseSubjectSpecialty of the original course
            var originalCSSList = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                css => css.CourseId == originalCourse.CourseId);
            
            // 3. Get all failed subjects in the original course
            var failedSubjects = new List<string>();
            foreach(var css in originalCSSList)
            {
                var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetAsync(
                    ta => ta.CourseSubjectSpecialtyId == css.Id && ta.TraineeId == traineeId);
                
                if (traineeAssign != null)
                {
                    var gradeForSubject = await _unitOfWork.GradeRepository.GetAsync(
                        g => g.TraineeAssignID == traineeAssign.TraineeAssignId);
                    
                    if (gradeForSubject != null && gradeForSubject.gradeStatus == GradeStatus.Fail)
                        failedSubjects.Add(css.SubjectId);
                }
            }
            
            // 4. No need to process if there are no failed subjects
            if (failedSubjects.Count == 0)
                return;
            
            // 5. Get all CourseSubjectSpecialty of the relearn course
            var relearnCSSList = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                css => css.CourseId == relearnCourse.CourseId);
            
            // 6. Check if student is assigned to the appropriate relearn course
            var traineeAssignment = await _unitOfWork.TraineeAssignRepository.GetAsync(
                ta => ta.TraineeAssignId == grade.TraineeAssignID);
            
            if (traineeAssignment == null)
                throw new InvalidOperationException("Student assignment not found");
                
            var relearnCSS = await _unitOfWork.CourseSubjectSpecialtyRepository.GetByIdAsync(
                traineeAssignment.CourseSubjectSpecialtyId);
            
            if (relearnCSS == null)
                throw new InvalidOperationException("CourseSubjectSpecialty information not found");
            
            // 7. Get the list of subjects in the relearn course
            var relearnSubjectIds = relearnCSSList
                .Where(css => css.SpecialtyId == relearnCSS.SpecialtyId)
                .Select(css => css.SubjectId)
                .ToList();
            
            // 8. Check if all failed subjects are included in the relearn course
            bool allFailedSubjectsIncluded = failedSubjects.All(s => relearnSubjectIds.Contains(s));
            if (!allFailedSubjectsIncluded)
            {
                throw new InvalidOperationException("Relearn course does not contain all failed subjects from the original course");
            }
            
            // 9. Check if all failed subjects are passed in the relearn course
            bool allRelearnSubjectsPassed = true;
            foreach(var subjectId in failedSubjects)
            {
                var cssForSubject = relearnCSSList.FirstOrDefault(css => 
                    css.SubjectId == subjectId && css.SpecialtyId == relearnCSS.SpecialtyId);
                
                if (cssForSubject != null)
                {
                    var traineeAssignForSubject = await _unitOfWork.TraineeAssignRepository.GetAsync(
                        ta => ta.CourseSubjectSpecialtyId == cssForSubject.Id && ta.TraineeId == traineeId);
                    
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
        
        // Handle aftermath for Professional course after relearn
        private async Task HandleProfessionalAfterRelearn(Course relearnCourse, Course rootCourse, string traineeId, string userId)
        {
            // Special handling for professional courses
            await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(
                relearnCourse.CourseId, userId);
                
            // Create decision for the course
            var decisionRequest = new CreateDecisionDTO { CourseId = relearnCourse.CourseId };
            await _decisionService.CreateDecisionForCourseAsync(decisionRequest, userId);
        }

        // Helper method to find the root course (Initial, Recurrent, or Professional)
        private async Task<Course> FindRootCourse(Course course)
        {
            if (course == null) 
                throw new InvalidOperationException("Course not found");
            
            // If not a relearn course, this is already a root course
            if (course.CourseLevel != CourseLevel.Relearn)
                return course;
            
            // Recursively trace back to find the root course
            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(course.RelatedCourseId);
            if (relatedCourse == null)
                throw new InvalidOperationException($"Related course not found for course: {course.CourseId}");
                
            return await FindRootCourse(relatedCourse);
        }

        // Handle aftermath for Initial course after relearn
        private async Task HandleInitialAfterRelearn(Course course, string traineeId, string userId)
        {
            // Generate certificates for passed trainees
            await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, userId);
            
            // Create decision for the course
            var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
            await _decisionService.CreateDecisionForCourseAsync(decisionRequest, userId);
        }

        // Handle aftermath for Recurrent course after relearn
        private async Task HandleRecurrentAfterRelearn(Course relearnCourse, Course rootCourse, string traineeId, string userId)
        {
            // Generate new certificates for passed trainees
            await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(relearnCourse.CourseId, userId);
            
            // Check if decision already exists
            var existingDecision = await _unitOfWork.DecisionRepository.GetAsync(
                d => d.Certificate.CourseId == relearnCourse.CourseId);
                
            if (existingDecision == null)
            {
                var decisionRequest = new CreateDecisionDTO { CourseId = relearnCourse.CourseId };
                await _decisionService.CreateDecisionForCourseAsync(decisionRequest, userId);
            }
                    }

        // Default aftermath handling for other course types
        private async Task HandleDefaultAfterRelearn(Course relearnCourse, string traineeId, string userId)
        {
            // Basic certificate generation for passed trainees
            await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(relearnCourse.CourseId, userId);
            
            // Create decision for the course
            var decisionRequest = new CreateDecisionDTO { CourseId = relearnCourse.CourseId };
            await _decisionService.CreateDecisionForCourseAsync(decisionRequest, userId);
        }

        // Add new helper method to check course completion and process certificates
        private async Task CheckAndProcessCourseCompletion(string courseId, string traineeId, string processedByUserId)
        {
            // Get the course
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            if (course == null) return;
            
            // Get all subjects in the course
            var allCourseSubjects = await _unitOfWork.CourseSubjectSpecialtyRepository.FindAsync(
                css => css.CourseId == courseId);
            
            // Get all trainee assignments for this trainee in this course
            var traineeAssignments = await _unitOfWork.TraineeAssignRepository.FindAsync(
                ta => ta.TraineeId == traineeId && 
                      allCourseSubjects.Select(cs => cs.Id).Contains(ta.CourseSubjectSpecialtyId));
            
            if (!traineeAssignments.Any()) return;
            
            // Check if all subjects have passing grades
            bool allSubjectsPassed = true;
            foreach (var assignment in traineeAssignments)
            {
                var grade = await _unitOfWork.GradeRepository.GetAsync(
                    g => g.TraineeAssignID == assignment.TraineeAssignId);
                
                if (grade == null || grade.gradeStatus != GradeStatus.Pass)
                {
                    allSubjectsPassed = false;
                    break;
                }
            }
            
            // If all subjects are passed, process certificates based on course type
            if (allSubjectsPassed)
            {
                // Check if certificate already exists
                var existingCertificate = await _unitOfWork.CertificateRepository.GetAsync(
                    c => c.UserId == traineeId && c.CourseId == courseId && c.Status == CertificateStatus.Active);
                    
                if (existingCertificate != null) return; // Certificate already exists
                
                // Handle special case for Relearn: Check if it's really a relearn situation
                if (course.CourseLevel == CourseLevel.Relearn)
                {
                    // If it's Relearn but related course doesn't exist or trainee has no records in related course
                    if (string.IsNullOrEmpty(course.RelatedCourseId))
                    {
                        // Treat as Initial course if no related course exists
                        await HandleInitialAfterRelearn(course, traineeId, processedByUserId);
                        return;
                    }
                    
                    // Check if the trainee has records in the related course
                    var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(course.RelatedCourseId);
                    if (relatedCourse == null)
                    {
                        // Treat as Initial course if related course doesn't exist in database
                        await HandleInitialAfterRelearn(course, traineeId, processedByUserId);
                        return;
                    }
                    
                    // Get all subjects in the related course
                    var relatedSubjects = await _unitOfWork.CourseSubjectSpecialtyRepository.FindAsync(
                        css => css.CourseId == relatedCourse.CourseId);
                    
                    // Check if trainee has assignments in related course
                    var traineeRelatedAssignments = await _unitOfWork.TraineeAssignRepository.FindAsync(
                        ta => ta.TraineeId == traineeId && 
                              relatedSubjects.Select(cs => cs.Id).Contains(ta.CourseSubjectSpecialtyId));
                    
                    if (!traineeRelatedAssignments.Any())
                    {
                        // No record of trainee in related course - treat as Initial course
                        await HandleInitialAfterRelearn(course, traineeId, processedByUserId);
                        return;
                    }
                    
                    // Continue with normal Relearn processing as trainee has records in related course
                    var rootCourse = await FindRootCourse(course);
                    switch (rootCourse.CourseLevel)
                    {
                        case CourseLevel.Initial:
                            await HandleInitialAfterRelearn(course, traineeId, processedByUserId);
                            break;
                        case CourseLevel.Recurrent:
                            await HandleRecurrentAfterRelearn(course, rootCourse, traineeId, processedByUserId);
                            break;
                        case CourseLevel.Professional:
                            await HandleProfessionalAfterRelearn(course, rootCourse, traineeId, processedByUserId);
                            break;
                        default:
                            await HandleDefaultAfterRelearn(course, traineeId, processedByUserId);
                            break;
                    }
                }
                else if (course.CourseLevel == CourseLevel.Initial)
                {
                    await HandleInitialAfterRelearn(course, traineeId, processedByUserId);
                }
                else if (course.CourseLevel == CourseLevel.Recurrent)
                {
                    await HandleRecurrentAfterRelearn(course, await FindRootCourse(course), traineeId, processedByUserId);
                }
                else if (course.CourseLevel == CourseLevel.Professional)
                {
                    await HandleProfessionalAfterRelearn(course, await FindRootCourse(course), traineeId, processedByUserId);
                }
            }
        }
        #endregion
    }
}
