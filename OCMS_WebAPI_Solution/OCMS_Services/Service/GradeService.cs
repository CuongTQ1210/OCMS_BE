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
        public GradeService(UnitOfWork unitOfWork, IMapper mapper, ICertificateService certificateService,ITrainingScheduleRepository trainingScheduleRepository, IDecisionService decisionService, IProgressTrackingService progressTrackingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _certificateService = certificateService;
            _trainingScheduleRepository = trainingScheduleRepository;
            _decisionService = decisionService;
            _progressTrackingService = progressTrackingService;
        }

        #region Get All Grade
        public async Task<IEnumerable<GradeModel>> GetAllAsync()
        {
            var grades = await _unitOfWork.GradeRepository.GetAllAsync();
            var gradeModels = new List<GradeModel>();

            foreach (var grade in grades)
            {
                // Fetch TraineeAssign and User data
                var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(grade.TraineeAssignID);
                var trainee = traineeAssign != null
                    ? await _unitOfWork.UserRepository.GetByIdAsync(traineeAssign.TraineeId)
                    : null;

                // Map Grade to GradeModel
                var gradeModel = _mapper.Map<GradeModel>(grade);
                gradeModel.Fullname = trainee?.FullName; // Set Fullname (null if trainee not found)

                gradeModels.Add(gradeModel);
            }

            return gradeModels;
        }
        #endregion

        #region Get Grade By ID
        public async Task<GradeModel> GetByIdAsync(string id)
        {
            var grade = await _unitOfWork.GradeRepository.GetByIdAsync(id);
            if (grade == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            // Fetch TraineeAssign and User data
            var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(grade.TraineeAssignID);
            var trainee = traineeAssign != null
                ? await _unitOfWork.UserRepository.GetByIdAsync(traineeAssign.TraineeId)
                : null;

            // Map Grade to GradeModel
            var gradeModel = _mapper.Map<GradeModel>(grade);
            gradeModel.Fullname = trainee?.FullName; // Set Fullname (null if trainee not found)

            return gradeModel;
        }
        #endregion

        #region Create grade TraineeAssignID
        public async Task<string> CreateAsync(GradeDTO dto, string gradedByUserId)
        {
            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(dto.SubjectId);
            if (subject == null)
            {
                throw new Exception("Subject not found.");
            }
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(subject.CourseId);
            if (course == null)
            {
                throw new Exception("Course not found.");
            }

            // Check if subject has any schedule
            var schedule = await _trainingScheduleRepository.GetSchedulesBySubjectIdAsync(dto.SubjectId);
            if (schedule == null)
            {
                throw new InvalidOperationException("Subject does not have any training schedule.");
            }
            
            if (course.Status== CourseStatus.Pending || course.Status == CourseStatus.Rejected || course.Progress==Progress.NotYet || course.Progress == Progress.Completed)
            {
                throw new InvalidOperationException("Course isn't suitable to create grade.");
            }
            // Check for existing grade with same TraineeAssignID and SubjectId
            var existingGrade = await _unitOfWork.GradeRepository
                .GetAsync(g => g.TraineeAssignID == dto.TraineeAssignID && g.SubjectId == dto.SubjectId);

            if (existingGrade != null)
                throw new InvalidOperationException("Grade for this trainee and subject already exists.");
            await ValidateGradeDto(dto);

            var grade = _mapper.Map<Grade>(dto);
            grade.GradeId = $"G-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            grade.GradedByInstructorId = gradedByUserId;
            
            var passScore = subject.PassingScore;
            grade.TotalScore = CalculateTotalScore(grade);

            if (grade.ParticipantScore == 0 || grade.AssignmentScore == 0)
            {
                grade.gradeStatus = GradeStatus.Fail;
            }
            else
            {
                grade.gradeStatus = grade.TotalScore >= passScore ? GradeStatus.Pass : GradeStatus.Fail;
            }
            await _unitOfWork.GradeRepository.AddAsync(grade);
            await _unitOfWork.SaveChangesAsync();
            if (grade.gradeStatus == GradeStatus.Pass)
            {
                var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(grade.TraineeAssignID);
                if (traineeAssign != null)
                {
                    var existingCertificates = await _unitOfWork.CertificateRepository
                        .GetAllAsync(c => c.CourseId == course.CourseId);
                    var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
                    bool traineeHasCert = traineeWithCerts.Contains(traineeAssign.TraineeId);

                    // 🧠 Decision logic based on course level
                    if (course.CourseLevel == CourseLevel.Initial)
                    {
                        if (!traineeHasCert)
                        {
                            // 1. Then Generate Certificate
                            await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, gradedByUserId);
                            // 2. Create Decision
                            var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                            await _decisionService.CreateDecisionForCourseAsync(decisionRequest, gradedByUserId);

                           
                           
                        }
                    }
                    else if (course.CourseLevel == CourseLevel.Recurrent)
                    {
                        await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, gradedByUserId);

                        var existingDecision = await _unitOfWork.DecisionRepository
                            .GetAsync(d => d.Certificate.CourseId == course.CourseId);

                        if (existingDecision == null)
                        {
                            var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                            await _decisionService.CreateDecisionForCourseAsync(decisionRequest, gradedByUserId);
                        }
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();

            // Kiểm tra và cập nhật trạng thái
            await _progressTrackingService.CheckAndUpdateSubjectStatus(dto.SubjectId);

            return grade.GradeId;
        }
        #endregion

        #region Update Grade By Id
        public async Task<bool> UpdateAsync(string id, GradeDTO dto)
        {
            var existing = await _unitOfWork.GradeRepository.GetAsync(g => g.GradeId == id);
            if (existing == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            var assignTrainee = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(dto.TraineeAssignID);
            if (assignTrainee == null)
                throw new KeyNotFoundException($"Trainee assignment with ID '{dto.TraineeAssignID}' not found.");

            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(dto.SubjectId);
            if (subject == null)
                throw new Exception("Subject not found.");

            var course = await _unitOfWork.CourseRepository.GetByIdAsync(assignTrainee.CourseId);
            if (course == null)
                throw new Exception("Course not found.");

            // Check if course is suitable for grading
            if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected ||
                course.Progress == Progress.NotYet || course.Progress == Progress.Completed)
            {
                throw new InvalidOperationException("Course isn't suitable to update grade.");
            }

            // Check for existing certificates
            var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.CourseId == course.CourseId);
            var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
            if (traineeWithCerts.Contains(assignTrainee.TraineeId))
                throw new InvalidOperationException($"Cannot update grade for TraineeAssignID '{existing.TraineeAssignID}' because a certificate has already been issued.");

            await ValidateGradeDto(dto);

            _mapper.Map(dto, existing);
            existing.TotalScore = CalculateTotalScore(existing);
            var passScore = subject.PassingScore;

            if (existing.ParticipantScore == 0 || existing.AssignmentScore == 0)
            {
                existing.gradeStatus = GradeStatus.Fail;
            }
            else
            {
                existing.gradeStatus = existing.TotalScore >= passScore ? GradeStatus.Pass : GradeStatus.Fail;
            }
            existing.UpdateDate = DateTime.Now;

            await _unitOfWork.GradeRepository.UpdateAsync(existing);

            if (existing.gradeStatus == GradeStatus.Pass)
            {
                // Certificate and decision logic based on course level
                if (course.CourseLevel == CourseLevel.Initial)
                {
                    if (!traineeWithCerts.Contains(assignTrainee.TraineeId))
                    {
                        // Generate certificate
                        await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, existing.GradedByInstructorId);
                        // Create decision
                        var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                        await _decisionService.CreateDecisionForCourseAsync(decisionRequest, existing.GradedByInstructorId);
                    }
                }
                else if (course.CourseLevel == CourseLevel.Recurrent)
                {
                    await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(course.CourseId, existing.GradedByInstructorId);

                    var existingDecision = await _unitOfWork.DecisionRepository
                        .GetAsync(d => d.Certificate.CourseId == course.CourseId);

                    if (existingDecision == null)
                    {
                        var decisionRequest = new CreateDecisionDTO { CourseId = course.CourseId };
                        await _decisionService.CreateDecisionForCourseAsync(decisionRequest, existing.GradedByInstructorId);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();        
            
            // Kiểm tra và cập nhật trạng thái
            await _progressTrackingService.CheckAndUpdateSubjectStatus(dto.SubjectId);

            return true;
        }
        #endregion

        #region Delete Grade By Id
        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await _unitOfWork.GradeRepository.GetAsync(g => g.GradeId == id);
            if (existing == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");
            var assignTrainee = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(existing.TraineeAssignID);
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(assignTrainee.CourseId);
            var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.CourseId == course.CourseId);
            var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
            if (traineeWithCerts.Any())
                throw new InvalidOperationException($"Cannot update grade for TraineeAssignID '{existing.TraineeAssignID}' because a certificate has already been issued.");
            await _unitOfWork.GradeRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion

        #region Get Grade By Status
        public async Task<List<GradeModel>> GetGradesByStatusAsync(GradeStatus status)
        {
            var grades = await _unitOfWork.GradeRepository.FindAsync(g => g.gradeStatus == status);
            return _mapper.Map<List<GradeModel>>(grades);
        }
        #endregion

        #region Get Grade By UserId (TraineeId)
        public async Task<List<GradeModel>> GetGradesByUserIdAsync(string userId)
        {
            var grades = await _unitOfWork.GradeRepository
                .FindIncludeAsync(g => g.TraineeAssign.TraineeId == userId, include => include.TraineeAssign);

            return _mapper.Map<List<GradeModel>>(grades);
        }
        #endregion

        #region Get Grade By SubjectId
        public async Task<List<GradeModel>> GetGradesBySubjectIdAsync(string subjectId)
        {
            var grades = await _unitOfWork.GradeRepository.FindAsync(g => g.SubjectId == subjectId);
            return _mapper.Map<List<GradeModel>>(grades);
        }
        #endregion

        #region Import Grades from Excel
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

                // Get subject by name
                var subject = await _unitOfWork.SubjectRepository.FirstOrDefaultAsync(s => s.SubjectName == subjectName);
                if (subject == null)
                {
                    result.Errors.Add($"Subject '{subjectName}' not found.");
                    return result;
                }

                string subjectId = subject.SubjectId;
                string courseId = subject.CourseId;
                // Get CourseId from subject
                var course = await _unitOfWork.CourseRepository.FirstOrDefaultAsync(c => c.CourseId == courseId);
                if (course == null)
                {
                    result.Errors.Add($"Course not found for Subject '{subjectName}'.");
                    return result;
                }

                // Validate course status
                if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected ||
                    course.Progress == Progress.NotYet || course.Progress == Progress.Completed)
                {
                    result.Errors.Add("Course isn't suitable to create grades.");
                    return result;
                }

                // Check if subject has a schedule
                var schedule = await _trainingScheduleRepository.GetSchedulesBySubjectIdAsync(subjectId);
                if (schedule == null)
                {
                    result.Errors.Add("Subject does not have any training schedule.");
                    return result;
                }

                var existingGrades = await _unitOfWork.GradeRepository.GetAllAsync();
                var existingGradeKeys = existingGrades.Select(g => (g.TraineeAssignID, g.SubjectId)).ToHashSet();

                var existingTraineeAssigns = await _unitOfWork.TraineeAssignRepository.GetAllAsync();
                var assignMap = existingTraineeAssigns
                    .Where(a => a.CourseId == courseId)
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
                        result.Errors.Add($"Row {row}: No TraineeAssign found for TraineeId '{traineeId}' in Course '{courseId}'.");
                        result.FailedCount++;
                        continue;
                    }

                    string assignId = assignData.TraineeAssignId;
                    string traineeUserId = assignData.TraineeId;

                    if (existingGradeKeys.Contains((assignId, subjectId)))
                    {
                        result.Errors.Add($"Row {row}: Grade already exists for TraineeAssignId '{assignId}' and Subject '{subjectId}'.");
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
                        SubjectId = subjectId,
                        ParticipantScore = participant,
                        AssignmentScore = assignment,
                        FinalExamScore = finalExam,
                        FinalResitScore = finalResit,
                        GradedByInstructorId = importedByUserId,
                        Remarks = remarks
                    };

                    var passScore = subject.PassingScore;
                    grade.TotalScore = CalculateTotalScore(grade);

                    if (grade.ParticipantScore == 0 || grade.AssignmentScore == 0)
                    {
                        grade.gradeStatus = GradeStatus.Fail;
                    }
                    else
                    {
                        grade.gradeStatus = grade.TotalScore >= passScore ? GradeStatus.Pass : GradeStatus.Fail;
                    }

                    newGrades.Add(grade);
                    result.SuccessCount++;
                }

                if (newGrades.Any())
                {
                    await _unitOfWork.GradeRepository.AddRangeAsync(newGrades);
                    await _unitOfWork.SaveChangesAsync();

                    // Process certificates and decisions for passing grades
                    try
                    {
                        var passingGrades = newGrades.Where(g => g.gradeStatus == GradeStatus.Pass).ToList();
                        if (passingGrades.Any())
                        {
                            // Get existing certificates for the course
                            var existingCertificates = await _unitOfWork.CertificateRepository
                                .GetAllAsync(c => c.CourseId == courseId);
                            var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));

                            if (course.CourseLevel == CourseLevel.Initial)
                            {
                                // Generate certificates only for trainees without existing certificates
                                foreach (var grade in passingGrades)
                                {
                                    var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(grade.TraineeAssignID);
                                    if (traineeAssign != null && !traineeWithCerts.Contains(traineeAssign.TraineeId))
                                    {
                                        await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(courseId, importedByUserId);
                                        // Create decision
                                        var decisionRequest = new CreateDecisionDTO { CourseId = courseId };
                                        await _decisionService.CreateDecisionForCourseAsync(decisionRequest, importedByUserId);
                                    }
                                }
                            }
                            else if (course.CourseLevel == CourseLevel.Recurrent)
                            {
                                // Generate certificates for all passing trainees
                                await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(courseId, importedByUserId);

                                // Create decision if none exists
                                var existingDecision = await _unitOfWork.DecisionRepository
                                    .GetAsync(d => d.Certificate.CourseId == courseId);
                                if (existingDecision == null)
                                {
                                    var decisionRequest = new CreateDecisionDTO { CourseId = courseId };
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

                    // Kiểm tra và cập nhật trạng thái cho tất cả subject có điểm mới
                    var affectedSubjectIds = newGrades.Select(g => g.SubjectId).Distinct().ToList();
                    foreach (var affectedSubjectId in affectedSubjectIds)
                    {
                        await _progressTrackingService.CheckAndUpdateSubjectStatus(affectedSubjectId);
                    }
                }
            }

            return result;
        }
        #endregion

        #region Helper Methods
        private double CalculateTotalScore(Grade grade)
        {
            double participant = grade.ParticipantScore * 0.1;
            double assignment = grade.AssignmentScore * 0.3;

            // Nếu có điểm resit > 0, dùng điểm đó. Nếu không, dùng điểm thi chính.
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

            if (string.IsNullOrEmpty(dto.SubjectId))
                throw new ArgumentException("SubjectId is required.");

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

            // Check existence of related data
            var traineeAssign = await _unitOfWork.TraineeAssignRepository.GetAsync(t => t.TraineeAssignId == dto.TraineeAssignID);
            if (traineeAssign == null)
                throw new InvalidOperationException("Trainee assignment not found.");

            var subject = await _unitOfWork.SubjectRepository.GetAsync(s => s.SubjectId == dto.SubjectId);
            if (subject == null)
                throw new InvalidOperationException("Subject not found.");
        }
        #endregion
    }
}
