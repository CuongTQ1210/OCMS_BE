using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Repositories.Repository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class TrainingPlanService : ITrainingPlanService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService; // Added for request creation
        private readonly ITrainingPlanRepository _trainingPlanRepository;
        public TrainingPlanService(UnitOfWork unitOfWork, IMapper mapper, IRequestService requestService, ITrainingPlanRepository trainingPlanRepository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
            _trainingPlanRepository = trainingPlanRepository ?? throw new ArgumentNullException(nameof(trainingPlanRepository));
        }

        #region Create Training
        public async Task<TrainingPlanModel> CreateTrainingPlanAsync(TrainingPlanDTO dto, string createUserId)
        {
            // Validate dates: EndDate must be after StartDate
            if (dto.EndDate <= dto.StartDate)
            {
                throw new ArgumentException("End date must be after start date.");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.PlanName))
            {
                throw new ArgumentException("Plan name is required.");
            }

            var trainingPlan = _mapper.Map<TrainingPlan>(dto);
            trainingPlan.PlanId = await GenerateTrainingPlanId(dto.StartDate);
            trainingPlan.Description = dto.Description;

            trainingPlan.CreateDate = DateTime.Now;
            trainingPlan.ModifyDate = DateTime.Now;
            trainingPlan.TrainingPlanStatus = TrainingPlanStatus.Draft;
            trainingPlan.CreateByUserId = createUserId;
            await _unitOfWork.TrainingPlanRepository.AddAsync(trainingPlan);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TrainingPlanModel>(trainingPlan);
        }
        #endregion

        #region Get Training plan by trainee user id
        public async Task<List<TrainingPlanModel>> GetTrainingPlansByTraineeIdAsync(string traineeId)
        {
            if (string.IsNullOrEmpty(traineeId))
                throw new ArgumentException("Trainee ID cannot be null or empty.", nameof(traineeId));

            var courseSubjectSpecialties = await _unitOfWork.CourseSubjectSpecialtyRepository
                .GetAllAsync(
                    css => css.Trainees.Any(t => t.TraineeId == traineeId),
                    includes: c => c.Course // Use includes parameter correctly
                );

            if (courseSubjectSpecialties == null || !courseSubjectSpecialties.Any())
            {
                throw new Exception("No training plan joined.");
            }

            // Get the course IDs from the CSS entities
            var courseIds = courseSubjectSpecialties
                .Where(css => css.Course != null)
                .Select(css => css.Course.CourseId)
                .Distinct()
                .ToList();

            // Fetch the training plans that reference these courses
            var trainingPlans = await _unitOfWork.TrainingPlanRepository
                .GetAllAsync(tp => courseIds.Contains(tp.CourseId));

            return _mapper.Map<List<TrainingPlanModel>>(trainingPlans);
        }
        #endregion

        #region Get all
        public async Task<IEnumerable<TrainingPlanModel>> GetAllTrainingPlansAsync()
        {
            var plans = await _unitOfWork.TrainingPlanRepository.GetAllAsync();

            // Additional queries to load related entities if needed
            var planIds = plans.Select(p => p.PlanId).ToList();
            var users = await _unitOfWork.UserRepository.GetAllAsync(
                u => plans.Any(p => p.CreateByUserId == u.UserId)
            );

            // Map the training plans
            var planModels = _mapper.Map<IEnumerable<TrainingPlanModel>>(plans);

            return planModels;
        }
        #endregion

        #region Get by id
        public async Task<TrainingPlanModel> GetTrainingPlanByIdAsync(string id)
        {
            var plan = await _unitOfWork.TrainingPlanRepository.GetAsync(p => p.PlanId == id);

            if (plan == null)
                return null;

            // If you need related data, load it separately
            var createUser = await _unitOfWork.UserRepository.GetByIdAsync(plan.CreateByUserId);

            var result = _mapper.Map<TrainingPlanModel>(plan);

            return result;

        }
        #endregion

        #region Get last id
        public async Task<TrainingPlanModel?> GetLastTrainingPlanAsync(string seasonCode, string year)
        {
            var lastTrainingPlan = await _unitOfWork.TrainingPlanRepository.GetLastObjectIdAsync(
                tp => tp.PlanId.StartsWith($"{seasonCode}{year}"),
                tp => tp.PlanId
            );

            return lastTrainingPlan == null ? null : _mapper.Map<TrainingPlanModel>(lastTrainingPlan);
        }
        #endregion

        #region Update Training Plan
        public async Task<TrainingPlanModel> UpdateTrainingPlanAsync(string id, TrainingPlanDTO dto, string updateUserId)
        {
            // Validate dates: EndDate must be after StartDate
            if (dto.EndDate <= dto.StartDate)
            {
                throw new ArgumentException("End date must be after start date.");
            }
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.PlanName))
            {
                throw new ArgumentException("Plan name is required.");
            }
            
            var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(id);
            if (trainingPlan == null)
                throw new KeyNotFoundException($"Training plan with ID {id} not found.");

            if (trainingPlan.TrainingPlanStatus != TrainingPlanStatus.Pending &&
                trainingPlan.TrainingPlanStatus != TrainingPlanStatus.Draft &&
                trainingPlan.TrainingPlanStatus != TrainingPlanStatus.Approved)
                throw new InvalidOperationException($"Cannot update training plan {id} because its status is {trainingPlan.TrainingPlanStatus}. Only Pending, Draft, or Approved plans can be updated.");

            if (trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved)
            {
                // Serialize the new data into Notes
                var proposedChanges = JsonSerializer.Serialize(dto);
                var requestDto = new RequestDTO
                {
                    RequestType = RequestType.PlanChange,
                    RequestEntityId = id,
                    Description = $"Request to update training plan {id}",
                    Notes = $"Proposed changes: {proposedChanges}" // Store new data in Notes
                };
                await _requestService.CreateRequestAsync(requestDto, updateUserId);
                return _mapper.Map<TrainingPlanModel>(trainingPlan); // Return unchanged plan
            }

            // Apply update for Pending or Draft - fix the typo and handle null values
            trainingPlan.PlanName = dto.PlanName;

            trainingPlan.Description = dto.Description; // Using existing property (still has typo)
            trainingPlan.StartDate = dto.StartDate;
            trainingPlan.EndDate = dto.EndDate;
            trainingPlan.ModifyDate = DateTime.Now;

            if (trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Updating)
            {
                trainingPlan.TrainingPlanStatus = TrainingPlanStatus.Pending;
            }
            await _unitOfWork.TrainingPlanRepository.UpdateAsync(trainingPlan);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TrainingPlanModel>(trainingPlan);
        }
        #endregion

        #region Delete Training Plan
        public async Task<bool> DeleteTrainingPlanAsync(string id)
        {
            var trainingPlan = await _unitOfWork.TrainingPlanRepository.GetByIdAsync(id);
            if (trainingPlan == null)
            {
                return false;
            }

            // Allow delete if status is Draft, Deleting, or Pending
            if (trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Draft ||
                trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Deleting ||
                trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Pending)
            {
                await _unitOfWork.TrainingPlanRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            // If Approved, send a request for approval
            if (trainingPlan.TrainingPlanStatus == TrainingPlanStatus.Approved)
            {
                var requestDto = new RequestDTO
                {
                    RequestType = RequestType.PlanDelete,
                    RequestEntityId = id,
                    Description = $"Request to delete training plan {id}",
                    Notes = "Awaiting HeadMaster approval"
                };

                await _requestService.CreateRequestAsync(requestDto, trainingPlan.CreateByUserId);
                throw new InvalidOperationException($"Cannot delete training plan {id} because it is Approved. A request has been sent to the HeadMaster for approval.");
            }

            // For other statuses (e.g., Rejected, Completed), block deletion
            throw new InvalidOperationException($"Cannot delete training plan {id} because its status is {trainingPlan.TrainingPlanStatus}. Only Draft, Deleting, or Pending plans can be deleted.");
        }
        #endregion

        #region Helper Methods
        private async Task<string> GenerateTrainingPlanId(DateTime trainingDate)
        {
            // Get year in two-digit format (e.g., 2025 -> 25)
            string shortYear = trainingDate.Year.ToString().Substring(2);

            // Get season from date
            string seasonCode = GetSeasonFromDate(trainingDate);

            // Get the last used number from the database using GenericRepository
            var lastTrainingPlan = await _unitOfWork.TrainingPlanRepository.GetLastObjectIdAsync(
                tp => tp.PlanId.StartsWith($"{seasonCode}{shortYear}"),
                tp => tp.PlanId
            );

            int nextNumber = 1;
            if (lastTrainingPlan != null)
            {
                var idParts = lastTrainingPlan.PlanId.Split('-');
                if (idParts.Length > 1 && int.TryParse(idParts.Last(), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // Format number as 3 digits (e.g., 001, 002)
            string formattedNumber = nextNumber.ToString("D3");

            // Construct the training plan ID
            return $"{seasonCode}{shortYear}-{formattedNumber}";
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
        #endregion
    }
}
