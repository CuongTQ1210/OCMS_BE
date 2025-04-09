using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Services.IService;
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

        public GradeService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GradeModel>> GetAllAsync()
        {
            var grades = await _unitOfWork.GradeRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GradeModel>>(grades);
        }

        public async Task<GradeModel> GetByIdAsync(string id)
        {
            var grade = await _unitOfWork.GradeRepository.GetByIdAsync(id);
            if (grade == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            return _mapper.Map<GradeModel>(grade);
        }

        public async Task<string> CreateAsync(GradeDTO dto, string gradedByUserId)
        {
            await ValidateGradeDto(dto);

            var grade = _mapper.Map<Grade>(dto);
            grade.GradeId = $"G-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            grade.GradedByInstructorId = gradedByUserId;

            grade.TotalScore = CalculateTotalScore(grade);

            if (grade.ParticipantScore == 0 || grade.AssignmentScore == 0)
            {
                grade.gradeStatus = GradeStatus.Fail;
            }
            else
            {
                grade.gradeStatus = grade.TotalScore >= 5.0 ? GradeStatus.Pass : GradeStatus.Fail;
            }
            await _unitOfWork.GradeRepository.AddAsync(grade);
            await _unitOfWork.SaveChangesAsync();

            return grade.GradeId;
        }

        public async Task<bool> UpdateAsync(string id, GradeDTO dto)
        {
            var existing = await _unitOfWork.GradeRepository.GetAsync(g => g.GradeId == id);
            if (existing == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

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

            _unitOfWork.GradeRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await _unitOfWork.GradeRepository.GetAsync(g => g.GradeId == id);
            if (existing == null)
                throw new KeyNotFoundException($"Grade with ID '{id}' not found.");

            _unitOfWork.GradeRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private double CalculateTotalScore(Grade grade)
        {
            double participant = grade.ParticipantScore * 0.1;
            double assignment = grade.AssignmentScore * 0.3;
            double final = Math.Max(grade.FinalExamScore, grade.FinalResitScore ?? 0) * 0.6;

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
    }
}
