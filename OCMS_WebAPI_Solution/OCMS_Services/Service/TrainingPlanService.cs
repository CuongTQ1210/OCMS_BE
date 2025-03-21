using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.Repository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class TrainingPlanService : ITrainingPlanService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public TrainingPlanService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        #region Create Training
        public async Task<TrainingPlanModel> CreateTrainingPlanAsync(TrainingPlanDTO dto, string createUserId)
        {

            var trainingPlan = _mapper.Map<TrainingPlan>(dto);
            trainingPlan.PlanId = await GenerateTrainingPlanId(dto.SpecialtyId, dto.StartDate, dto.PlanLevel);
            trainingPlan.CreateDate = DateTime.UtcNow;
            trainingPlan.ModifyDate = DateTime.UtcNow;
            trainingPlan.TrainingPlanStatus = TrainingPlanStatus.Pending;
            trainingPlan.CreateByUserId = createUserId;
            _unitOfWork.TrainingPlanRepository.AddAsync(trainingPlan);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TrainingPlanModel>(trainingPlan);
        }
        #endregion

        #region Get all
        public async Task<IEnumerable<TrainingPlanModel>> GetAllTrainingPlansAsync()
        {
            var plans = await _unitOfWork.TrainingPlanRepository.GetAllAsync(
                p => p.CreateByUser,
                p => p.Specialty
            );

            return _mapper.Map<IEnumerable<TrainingPlanModel>>(plans);
        }
        #endregion

        #region get by id 

        public async Task<TrainingPlanModel> GetTrainingPlanByIdAsync(string id)
        {
            var plan = await _unitOfWork.TrainingPlanRepository.GetAsync(
                p => p.PlanId == id,
                p => p.CreateByUser,
                p => p.Specialty
            );

            return plan == null ? null : _mapper.Map<TrainingPlanModel>(plan);
        }
        #endregion

        #region get last id 
        public async Task<TrainingPlanModel?> GetLastTrainingPlanAsync(string specialtyId, string seasonCode, string year, PlanLevel planLevel)
        {
            var lastTrainingPlan = await _unitOfWork.TrainingPlanRepository.GetLastObjectIdAsync(
                tp => ((TrainingPlan)(object)tp).SpecialtyId == specialtyId &&
                      ((TrainingPlan)(object)tp).PlanId.StartsWith($"{specialtyId}-{seasonCode}{year}") &&
                      ((TrainingPlan)(object)tp).PlanLevel == planLevel,
                tp => ((TrainingPlan)(object)tp).PlanId
            );

            return _mapper.Map<TrainingPlanModel>(lastTrainingPlan);
        }
        #endregion

        #region update training plan

        public async Task<TrainingPlanModel> UpdateTrainingPlanAsync(string id, TrainingPlanDTO dto, string updateUserId)
        {
            var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetAsync(p => p.PlanId == id);

            if (trainingPlan == null)
            {
                throw new KeyNotFoundException("Training plan not found.");
            }

            // Update fields
            trainingPlan.PlanName = dto.PlanName;
            trainingPlan.Desciption = dto.Desciption;
            trainingPlan.ModifyDate = DateTime.UtcNow;
            trainingPlan.CreateByUserId = updateUserId;

            await _unitOfWork.TrainingPlanRepository.UpdateAsync(trainingPlan);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TrainingPlanModel>(trainingPlan);
        }
        #endregion

        #region delete 

        public async Task<bool> DeleteTrainingPlanAsync(string id)
        {
            var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(id);

            if (trainingPlan == null)
            {
                return false;
            }

            await _unitOfWork.TrainingPlanRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        #endregion

        #region create plan id
        private async Task<string> GenerateTrainingPlanId(string specialtyId, DateTime trainingDate, PlanLevel planLevel)
        {
            // Get year in two-digit format (e.g., 2025 -> 25)
            string shortYear = trainingDate.Year.ToString().Substring(2);

            // Get season from date
            string seasonCode = GetSeasonFromDate(trainingDate);

            // Convert plan level to string code
            string planLevelCode = GetPlanLevelCode(planLevel);

            // Get the last used number from the database using GenericRepository
            var lastTrainingPlan = await _unitOfWork.TrainingPlanRepository.GetLastObjectIdAsync(
                tp => tp.SpecialtyId == specialtyId &&
                      tp.PlanId.StartsWith($"{specialtyId}-{seasonCode}{shortYear}-{planLevelCode}"),
                tp => tp.PlanId
            );

            int nextNumber = 1;
            if (lastTrainingPlan != null)
            {
                var idParts = lastTrainingPlan.PlanId.Split('-');
                if (idParts.Length > 2 && int.TryParse(idParts.Last(), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // Format number as 3 digits (e.g., 001, 002)
            string formattedNumber = nextNumber.ToString("D3");

            // Construct the training plan ID
            return $"{specialtyId}-{seasonCode}{shortYear}-{planLevelCode}-{formattedNumber}";
        }

        private string GetSeasonFromDate(DateTime date)
        {
            int month = date.Month;

            if (month >= 3 && month <= 5)
                return "SP"; // Spring (March - May)
            else if (month >= 6 && month <= 8)
                return "SU"; // Summer (June - August)
            else if (month >= 9 && month <= 11)
                return "FA"; // Fall (September - November)
            else
                return "WT"; // Winter (December - February)
        }
        private string GetPlanLevelCode(PlanLevel planLevel)
        {
            return planLevel switch
            {
                PlanLevel.Initial => "INI",
                PlanLevel.Recurrent => "REC",
                PlanLevel.Relearn => "REL",
                _ => throw new ArgumentOutOfRangeException(nameof(planLevel), "Invalid plan level")
            };
        }
        #endregion
    }
}
