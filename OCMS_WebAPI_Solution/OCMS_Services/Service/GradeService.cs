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

            if (grade.gradeStatus == GradeStatus.Pass)
            {
                var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                    c => c.CourseId == course.CourseId);
                var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
                bool traineeHasCert = traineeWithCerts.Contains(traineeAssign.TraineeId);

                if (course.CourseLevel == CourseLevel.Initial)
                {
                    if (!traineeHasCert)
                    {
                        await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, gradedByUserId);
                        var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                        await _decisionService.CreateDecisionForCourseAsync(decisionRequest, gradedByUserId);
                    }
                }
                else if (course.CourseLevel == CourseLevel.Recurrent)
                {
                    await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, gradedByUserId);
                    var existingDecision = await _unitOfWork.DecisionRepository.GetAsync(
                        d => d.Certificate.CourseId == course.CourseId);
                    if (existingDecision == null)
                    {
                        var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                        await _decisionService.CreateDecisionForCourseAsync(decisionRequest, gradedByUserId);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
            //await _progressTrackingService.CheckAndUpdateCourseSubjectStatus(traineeAssign.CourseSubjectSpecialtyId);

            return grade.GradeId;
        }

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
            //await _progressTrackingService.CheckAndUpdateCourseSubjectStatus(assignTrainee.CourseSubjectSpecialtyId);

            return true;
        }

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

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets["GradeImport"];
                if (worksheet == null)
                {
                    result.Errors.Add("Missing 'GradeImport' sheet.");
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
                    css => css.Instructors);
                if (!cssList.Any())
                {
                    result.Errors.Add($"No CourseSubjectSpecialty found for Subject '{subjectName}'.");
                    return result;
                }

                var instructorAssign = cssList
                    .SelectMany(css => css.Instructors)
                    .FirstOrDefault(ia => ia.InstructorId == importedByUserId);
                if (instructorAssign == null)
                    throw new InvalidOperationException("User is not authorized to grade this subject.");

                var css = cssList.First();
                var course = css.Course;
                if (course == null)
                {
                    result.Errors.Add($"Course not found for Subject '{subjectName}'.");
                    return result;
                }

                if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected ||
                    course.Progress == Progress.NotYet || course.Progress == Progress.Completed)
                {
                    result.Errors.Add("Course isn't suitable to create grades.");
                    return result;
                }

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

                var existingTraineeAssigns = await _unitOfWork.TraineeAssignRepository.GetAllAsync(
                    t => t.CourseSubjectSpecialty,
                    t => t.CourseSubjectSpecialty.Course);
                var assignMap = existingTraineeAssigns
                    .Where(a => a.CourseSubjectSpecialty.CourseId == course.CourseId)
                    .ToDictionary(a => a.TraineeId, a => (a.TraineeAssignId, a.TraineeId));

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
                        result.Errors.Add($"Row {row}: No TraineeAssign found for TraineeId '{traineeId}' in Course '{course.CourseId}'.");
                        result.FailedCount++;
                        continue;
                    }

                    string assignId = assignData.TraineeAssignId;
                    string traineeUserId = assignData.TraineeId;

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
                            var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                                c => c.CourseId == course.CourseId);
                            var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));

                            if (course.CourseLevel == CourseLevel.Initial)
                            {
                                foreach (var grade in passingGrades)
                                {
                                    var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(grade.TraineeAssignID);
                                    if (traineeAssign != null && !traineeWithCerts.Contains(traineeAssign.TraineeId))
                                    {
                                        await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, importedByUserId);
                                    }
                                    var newCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(
                                        c => c.CourseId == course.CourseId && c.Status == CertificateStatus.Pending);
                                    if (newCertificates.Any() && !traineeWithCerts.Contains(newCertificates.First().UserId))
                                    {
                                        var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                                        await _decisionService.CreateDecisionForCourseAsync(decisionRequest, importedByUserId);
                                    }
                                }
                            }
                            else if (course.CourseLevel == CourseLevel.Recurrent)
                            {
                                await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, importedByUserId);
                                var existingDecision = await _unitOfWork.DecisionRepository.GetAsync(
                                    d => d.Certificate.CourseId == course.CourseId);
                                if (existingDecision == null)
                                {
                                    var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                                    await _decisionService.CreateDecisionForCourseAsync(decisionRequest, importedByUserId);
                                }
                            }

                            result.AdditionalInfo = $"Successfully processed {passingGrades.Count} passing grades with certificate and decision generation.";
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"Grades were imported successfully, but certificate/decision generation failed: {ex.Message}");
                    }

                    //var affectedSubjectIds = new List<string> { subject.SubjectId };
                    //foreach (var affectedSubjectId in affectedSubjectIds)
                    //{
                    //    await _progressTrackingService.CheckAndUpdateCourseSubjectStatus(assignTrainee.CourseSubjectSpecialtyId);
                    //}
                }
            }

            return result;
        }

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
    }
}
