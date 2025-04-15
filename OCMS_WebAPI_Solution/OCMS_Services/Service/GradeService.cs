using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
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

        public GradeService(UnitOfWork unitOfWork, IMapper mapper, ICertificateService certificateService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _certificateService = certificateService;
        }

        #region Get All Grade
        public async Task<IEnumerable<GradeModel>> GetAllAsync()
        {
            var grades = await _unitOfWork.GradeRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GradeModel>>(grades);
        }
        #endregion

        #region Get Grade By ID
        public async Task<GradeModel> GetByIdAsync(string id)
        {
            var grade = await _unitOfWork.GradeRepository.GetByIdAsync(id);
            if (grade == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            return _mapper.Map<GradeModel>(grade);
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
            if (subject.Schedules == null || !subject.Schedules.Any())
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
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(assignTrainee.CourseId);
            var existingCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.CourseId == course.CourseId);
            var traineeWithCerts = new HashSet<string>(existingCertificates.Select(c => c.UserId));
            if (traineeWithCerts.Any())
                throw new InvalidOperationException($"Cannot update grade for TraineeAssignID '{existing.TraineeAssignID}' because a certificate has already been issued.");
            await ValidateGradeDto(dto);

            _mapper.Map(dto, existing);
            existing.TotalScore = CalculateTotalScore(existing);
            if (existing.ParticipantScore == 0 || existing.AssignmentScore == 0)
            {
                existing.gradeStatus = GradeStatus.Fail;
            }
            else
            {
                existing.gradeStatus = existing.TotalScore >= 5.0 ? GradeStatus.Pass : GradeStatus.Fail;
            }
            existing.UpdateDate = DateTime.UtcNow;

            await _unitOfWork.GradeRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

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
                Errors = new List<string>()
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
                if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected || course.Progress == Progress.NotYet || course.Progress == Progress.Completed)
                {
                    throw new InvalidOperationException("Course isn't suitable to create grade.");
                }
                var existingGrades = await _unitOfWork.GradeRepository.GetAllAsync();
                var existingGradeKeys = existingGrades.Select(g => (g.TraineeAssignID, g.SubjectId)).ToHashSet();

                var existingTraineeAssigns = await _unitOfWork.TraineeAssignRepository.GetAllAsync();
                var assignMap = existingTraineeAssigns
                    .Where(a => a.CourseId == courseId)
                    .ToDictionary(a => a.TraineeId, a => a.TraineeAssignId);
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

                    if (!assignMap.TryGetValue(traineeId, out string assignId))
                    {
                        result.Errors.Add($"Row {row}: No TraineeAssign found for TraineeId '{traineeId}' in Course '{courseId}'.");
                        result.FailedCount++;
                        continue;
                    }

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
                        TraineeAssignID = assignId,
                        SubjectId = subjectId,
                        ParticipantScore = participant,
                        AssignmentScore = assignment,
                        FinalExamScore = finalExam,
                        FinalResitScore = finalResit,
                        GradedByInstructorId= importedByUserId,
                        Remarks = remarks
                    };
                    grade.GradeId = $"G-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
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

                    // Improve logging before certificate generation
                    // Check and automatically generate certificates for eligible trainees
                    try
                    {
                        var certificates = await _certificateService.AutoGenerateCertificatesForPassedTraineesAsync(courseId, importedByUserId);
                        if (certificates.Any())
                        {
                            result.AdditionalInfo = $"Successfully generated {certificates.Count} certificates for passing trainees.";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the grade import process
                        result.Warnings = result.Warnings ?? new List<string>();
                        result.Warnings.Add($"Grades were imported successfully but certificate generation failed: {ex.Message}");
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
