using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.ResponseModel;
using OCMS_Repositories.IRepository;
using OCMS_Repositories;
using OCMS_Services.IService;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OCMS_BOs.ViewModel;
using OCMS_BOs.RequestModel;
using Microsoft.EntityFrameworkCore;

namespace OCMS_Services.Service
{
    public class TraineeAssignService : ITraineeAssignService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ITraineeAssignRepository _traineeAssignRepository;
        private readonly ICourseSubjectSpecialtyRepository _courseSubjectSpecialtyRepository;

        public TraineeAssignService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            ICourseRepository courseRepository,
            ITraineeAssignRepository traineeAssignRepository,
            ICourseSubjectSpecialtyRepository courseSubjectSpecialtyRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
            _courseRepository = courseRepository;
            _traineeAssignRepository = traineeAssignRepository;
            _courseSubjectSpecialtyRepository = courseSubjectSpecialtyRepository;
        }

        #region Get All Trainee Assignments
        public async Task<IEnumerable<TraineeAssignModel>> GetAllTraineeAssignmentsAsync()
        {
            var assignments = await _unitOfWork.TraineeAssignRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TraineeAssignModel>>(assignments);
        }
        #endregion

        #region Get Trainee Assignment By ID
        public async Task<TraineeAssignModel> GetTraineeAssignmentByIdAsync(string traineeAssignId)
        {
            if (string.IsNullOrEmpty(traineeAssignId))
                throw new ArgumentException("Trainee Assignment ID cannot be null or empty.", nameof(traineeAssignId));

            var assignment = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(traineeAssignId);
            if (assignment == null)
                throw new KeyNotFoundException($"Trainee Assignment with ID {traineeAssignId} not found.");

            return _mapper.Map<TraineeAssignModel>(assignment);
        }
        #endregion

        #region Update Trainee Assignment
        public async Task<TraineeAssignModel> UpdateTraineeAssignmentAsync(string id, TraineeAssignDTO dto)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Assignment ID cannot be null or empty.", nameof(id));

            var existingAssignment = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(id);
            if (existingAssignment == null)
                throw new KeyNotFoundException($"Trainee Assignment with ID {id} not found.");

            // Ensure update is allowed only if status is "Pending" or "Rejected"
            if (existingAssignment.RequestStatus.ToString() != "Pending" && existingAssignment.RequestStatus.ToString() != "Rejected")
                throw new InvalidOperationException($"Cannot update trainee assignment because its status is '{existingAssignment.RequestStatus.ToString()}'. Only 'Pending' or 'Rejected' can be updated.");

            // Validate TraineeId
            var trainee = await _unitOfWork.UserRepository.GetByIdAsync(dto.TraineeId);
            if (trainee == null)
                throw new Exception($"Trainee with ID {dto.TraineeId} not found.");

            // Validate that the user is a Trainee
            if (trainee.RoleId != 7) // Assuming 7 = Trainee
                throw new Exception($"User with ID {dto.TraineeId} is not a Trainee. Role: {trainee.Role}.");

            // Validate CourseSubjectSpecialtyId
            var css = await _unitOfWork.CourseSubjectSpecialtyRepository.GetByIdAsync(dto.CourseSubjectSpecialtyId);
            if (css == null)
                throw new Exception($"CourseSubjectSpecialty with ID {dto.CourseSubjectSpecialtyId} not found.");

            var course = await _unitOfWork.CourseRepository.GetByIdAsync(css.CourseId);
            if (course == null)
                throw new Exception($"Course with ID {css.CourseId} not found.");

            var trainingPlan = await _unitOfWork.TrainingPlanRepository
    .FindAsync(tp => tp.CourseId == course.CourseId);

            var trainingPlanEntity = trainingPlan.FirstOrDefault();

            if (trainingPlanEntity == null)
                throw new Exception($"Training Plan with CourseID {course.CourseId} not found.");

            // Kiểm tra nếu Trainee có Specialty phù hợp với TrainingPlan
            if (trainee.SpecialtyId != css.SpecialtyId)
                throw new Exception($"Trainee's specialty ({trainee.SpecialtyId}) does not match with the Training Plan's specialty ({css.SpecialtyId}).");

            // Check if the trainee is already assigned to this CourseSubjectSpecialty (only if changing the trainee or CourseSubjectSpecialty)
            if (dto.TraineeId != existingAssignment.TraineeId || dto.CourseSubjectSpecialtyId != existingAssignment.CourseSubjectSpecialtyId)
            {
                var existingAssignmentCheck = await _unitOfWork.TraineeAssignRepository
                    .FindAsync(ta => ta.TraineeId == dto.TraineeId && ta.CourseSubjectSpecialtyId == dto.CourseSubjectSpecialtyId && ta.TraineeAssignId != id);
                if (existingAssignmentCheck.Any())
                    throw new Exception($"Trainee {dto.TraineeId} is already assigned to CourseSubjectSpecialty {dto.CourseSubjectSpecialtyId}.");
            }

            // Update the IsAssign status of the user if needed
            if (trainee != null && !trainee.IsAssign)
            {
                trainee.IsAssign = true;
                await _unitOfWork.UserRepository.UpdateAsync(trainee);
            }

            // Update the assignment
            existingAssignment.TraineeId = dto.TraineeId;
            existingAssignment.CourseSubjectSpecialtyId = dto.CourseSubjectSpecialtyId;
            existingAssignment.Notes = dto.Notes;

            await _unitOfWork.TraineeAssignRepository.UpdateAsync(existingAssignment);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TraineeAssignModel>(existingAssignment);
        }
        #endregion

        #region Delete Trainee Assignment
        public async Task<(bool success, string message)> DeleteTraineeAssignmentAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Invalid ID.");

            var assignment = await _unitOfWork.TraineeAssignRepository.GetByIdAsync(id);
            if (assignment == null)
                return (false, "Trainee Assignment not found.");

            // Ensure deletion is allowed only if status is "Pending" or "Rejected"
            if (assignment.RequestStatus.ToString() != "Pending" && assignment.RequestStatus.ToString() != "Rejected")
                return (false, $"Cannot delete trainee assignment because its status is '{assignment.RequestStatus.ToString()}'. Only 'Pending' or 'Rejected' can be deleted. Please Send Delete Request to delete");

            await _unitOfWork.TraineeAssignRepository.DeleteAsync(id);
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == assignment.TraineeId);
            if (user != null)
            {
                user.IsAssign = false;
                await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            await _unitOfWork.SaveChangesAsync();

            return (true, "Trainee Assignment deleted successfully.");
        }
        #endregion

        #region Get Trainee's Assigned Courses
        public async Task<IEnumerable<CourseModel>> GetCoursesByTraineeIdAsync(string traineeId)
        {
            if (string.IsNullOrEmpty(traineeId))
                throw new ArgumentException("Trainee ID cannot be null or empty.", nameof(traineeId));

            var assignments = await _unitOfWork.TraineeAssignRepository.GetAllAsync(a => a.TraineeId == traineeId);
            var cssIds = assignments.Select(a => a.CourseSubjectSpecialtyId).Distinct().ToList();
            var cssList = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(css => cssIds.Contains(css.Id));
            var courseIds = cssList.Select(css => css.CourseId).Distinct().ToList();
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(c => courseIds.Contains(c.CourseId));

            return _mapper.Map<IEnumerable<CourseModel>>(courses);
        }
        #endregion

        #region Create TraineeAssign
        public async Task<TraineeAssignModel> CreateTraineeAssignAsync(TraineeAssignDTO dto, string createdByUserId)
        {
            // Validate TraineeId (UserId)
            var trainee = await _unitOfWork.UserRepository.GetByIdAsync(dto.TraineeId);
            if (trainee == null)
            {
                throw new Exception($"Trainee with ID {dto.TraineeId} not found.");
            }

            // Validate that the user is a Trainee
            if (trainee.RoleId != 7) // Assuming 7 = Trainee
            {
                throw new Exception($"User with ID {dto.TraineeId} is not a Trainee. Role: {trainee.Role}.");
            }

            // Validate CourseSubjectSpecialtyId
            var css = await _unitOfWork.CourseSubjectSpecialtyRepository.GetByIdAsync(dto.CourseSubjectSpecialtyId);
            if (css == null)
            {
                throw new Exception($"CourseSubjectSpecialty with ID {dto.CourseSubjectSpecialtyId} not found.");
            }

            var course = await _unitOfWork.CourseRepository.GetByIdAsync(css.CourseId);
            if (course == null)
            {
                throw new Exception($"Course with ID {css.CourseId} not found.");
            }
            if (course.Status != CourseStatus.Approved)
            {
                throw new Exception("Course hasn't been approved yet!");
            }

            var trainingPlan = await _unitOfWork.TrainingPlanRepository
    .FindAsync(tp => tp.CourseId == course.CourseId);

            var trainingPlanEntity = trainingPlan.FirstOrDefault();

            if (trainingPlanEntity == null)
                throw new Exception($"Training Plan with CourseID {course.CourseId} not found.");

            // Kiểm tra nếu Trainee có Specialty phù hợp với TrainingPlan
            if (trainee.SpecialtyId != css.SpecialtyId)
            {
                throw new Exception($"Trainee's specialty ({trainee.SpecialtyId}) does not match with the Training Plan's specialty ({css.SpecialtyId}).");
            }

            // Check if the trainee is already assigned to this CourseSubjectSpecialty
            var existingAssignment = await _unitOfWork.TraineeAssignRepository
                .FindAsync(ta => ta.TraineeId == dto.TraineeId && ta.CourseSubjectSpecialtyId == dto.CourseSubjectSpecialtyId);
            if (existingAssignment.Any())
            {
                throw new Exception($"Trainee {dto.TraineeId} is already assigned to CourseSubjectSpecialty {dto.CourseSubjectSpecialtyId}.");
            }

            // Generate unique TraineeAssignId
            var lastTraineeAssignId = await GetLastTraineeAssignIdAsync();
            int lastIdNumber = 0;
            if (!string.IsNullOrEmpty(lastTraineeAssignId))
            {
                string numericPart = new string(lastTraineeAssignId.Where(char.IsDigit).ToArray());
                int.TryParse(numericPart, out lastIdNumber);
            }
            lastIdNumber++;
            string newTraineeAssignId = $"TA{lastIdNumber:D5}";

            //Create a new Request for approval
            var newRequest = new Request
            {
                RequestId = $"REQ-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                RequestType = RequestType.AddTraineeAssign,
                RequestUserId = createdByUserId,
                RequestDate = DateTime.UtcNow,
                Status = RequestStatus.Pending,
                Description = $"Assign trainee {dto.TraineeId} to CourseSubjectSpecialty {dto.CourseSubjectSpecialtyId}.",
                Notes = $"Request to assign Trainee {dto.TraineeId} to CourseSubjectSpecialty {dto.CourseSubjectSpecialtyId}."
            };

            //Create TraineeAssign object with RequestId
            var traineeAssign = new TraineeAssign
            {
                TraineeAssignId = newTraineeAssignId,
                TraineeId = dto.TraineeId,
                CourseSubjectSpecialtyId = dto.CourseSubjectSpecialtyId,
                RequestId = newRequest.RequestId,
                AssignByUserId = createdByUserId,
                RequestStatus = RequestStatus.Pending,
                AssignDate = DateTime.UtcNow,
                ApprovalDate = null,
                ApproveByUserId = null,
                Notes = dto.Notes
            };

            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == dto.TraineeId);
            if (user != null)
            {
                user.IsAssign = true;
                await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            //Save both Request & TraineeAssign in a single transaction
            await _unitOfWork.RequestRepository.AddAsync(newRequest);
            await _unitOfWork.TraineeAssignRepository.AddAsync(traineeAssign);
            await _unitOfWork.SaveChangesAsync();

            //Return TraineeAssignModel
            return _mapper.Map<TraineeAssignModel>(traineeAssign);
        }
        #endregion

        #region Import TraineeAssignments from Excel
        public async Task<ImportResult> ImportTraineeAssignmentsFromExcelAsync(Stream fileStream, string importedByUserId)
        {
            var result = new ImportResult
            {
                TotalRecords = 0,
                SuccessCount = 0,
                FailedCount = 0,
                Errors = new List<string>()
            };

            try
            {
                var existingCss = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                    css => css.Course);

                // Now, separately get all training plans for the related course IDs
                var courseIds = existingCss.Select(css => css.Course.CourseId).Distinct().ToList();

                var trainingPlans = await _unitOfWork.TrainingPlanRepository
                    .FindAsync(tp => courseIds.Contains(tp.CourseId));
                var existingCssIds = existingCss.Select(css => css.Id).ToList();
                var cssDict = existingCss.ToDictionary(css => css.Id, css => css);

                var existingUsers = await _unitOfWork.UserRepository.GetAllAsync();
                var userDict = existingUsers.ToDictionary(u => u.UserId, u => u);

                var existingAssignments = await _unitOfWork.TraineeAssignRepository.GetAllAsync();
                var existingAssignmentPairs = existingAssignments
                    .Select(ta => (ta.TraineeId, ta.CourseSubjectSpecialtyId))
                    .ToHashSet();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets["TraineeAssign"];
                    if (worksheet == null)
                    {
                        result.Errors.Add("Excel file must contain a 'TraineeAssign' sheet.");
                        return result;
                    }

                    // Read CourseSubjectSpecialtyId from cell B1
                    string courseId = worksheet.Cells[1, 2].GetValue<string>(); // B1 (row 1, column 2)
                    if (string.IsNullOrEmpty(courseId))
                    {
                        result.Errors.Add($"Invalid or missing CourseId '{courseId}' in cell B1.");
                        return result;
                    }
                    var courses = await _unitOfWork.CourseRepository.FirstOrDefaultAsync(css => css.CourseId == courseId);
                    if(courses == null)    
                    {
                        result.Errors.Add($"Dont have Course that have CourseID: '{courseId}' in cell B1.");
                        return result;

                    }
                    string subjectId = worksheet.Cells[1, 4].GetValue<string>(); 
                    if (string.IsNullOrEmpty(subjectId))
                    {
                        result.Errors.Add($"Invalid or missing SubjectId '{subjectId}' in cell D1.");
                        return result;
                    }
                    var subjects = await _unitOfWork.SubjectRepository.FirstOrDefaultAsync(css => css.SubjectId == subjectId);

                    if (subjects == null)
                    {
                        result.Errors.Add($"Dont have Subject that have SubjectId: '{subjects}' in cell D1.");
                        return result;

                    }
                    string SpecialtyId = worksheet.Cells[1, 6].GetValue<string>(); 
                    if (string.IsNullOrEmpty(SpecialtyId))
                    {
                        result.Errors.Add($"Invalid or missing SpecialtyId '{SpecialtyId}' in cell F1.");
                        return result;
                    }
                    var Specialtys = await _unitOfWork.SpecialtyRepository.FirstOrDefaultAsync(css => css.SpecialtyId == SpecialtyId);

                    if (Specialtys == null)
                    {
                        result.Errors.Add($"Dont have Specialty that have SpecialtyID: '{SpecialtyId}' in cell F1.");
                        return result;

                    }
                    var cssEntity = await _unitOfWork.CourseSubjectSpecialtyRepository.FirstOrDefaultAsync(
                        css => css.CourseId == courseId && css.SubjectId == subjectId && css.SpecialtyId == SpecialtyId);

                    if (cssEntity == null)
                    {
                        result.Errors.Add($"Không tìm thấy CourseSubjectSpecialty với CourseId={courseId}, SubjectId={subjectId}, SpecialtyId={SpecialtyId}");
                        return result;
                    }

                    var cssId = cssEntity.Id; var css = cssDict[cssId];
                    var course = css.Course;

                    if (course.Status != CourseStatus.Approved)
                    {
                        throw new Exception("Course hasn't been approved yet!");
                    }

                    // Fetch all training plans that use this course
                    var relevantTrainingPlans = trainingPlans.Where(tp => tp.CourseId == course.CourseId).ToList();
                    if (!relevantTrainingPlans.Any())
                    {
                        throw new Exception($"No training plan found for course ID {course.CourseId}");
                    }

                    var lastTraineeAssignId = await GetLastTraineeAssignIdAsync();
                    int lastIdNumber = 0;
                    if (!string.IsNullOrEmpty(lastTraineeAssignId))
                    {
                        string numericPart = new string(lastTraineeAssignId.Where(char.IsDigit).ToArray());
                        int.TryParse(numericPart, out lastIdNumber);
                    }

                    var traineeAssignments = new List<TraineeAssign>();
                    var processedUserIds = new HashSet<string>();
                    int rowCount = worksheet.Dimension.Rows;
                    result.TotalRecords = rowCount - 2; // Starting from row 3, so subtract 2 (row 1 and 2 are headers)

                    // Start from row 3
                    for (int row = 3; row <= rowCount; row++)
                    {
                        if (IsRowEmpty(worksheet, row)) continue;

                        string userId = worksheet.Cells[row, 1].GetValue<string>(); // Column A (A3, A4, A5, ...)
                        if (string.IsNullOrEmpty(userId))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: UserId is missing.");
                            continue;
                        }

                        if (!userDict.ContainsKey(userId))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: User with ID '{userId}' does not exist.");
                            continue;
                        }

                        var user = userDict[userId];
                        if (user.RoleId != 7)
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: User with ID '{userId}' is not a Trainee. Role: {user.RoleId}.");
                            continue;
                        }

                        if (existingAssignmentPairs.Contains((userId, cssId)))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: Trainee '{userId}' is already assigned to CourseSubjectSpecialty '{cssId}'.");
                            continue;
                        }

                        // Validate the specialty ID for each training plan
                        bool specialtyMismatch = false;
                        foreach (var trainingPlan in relevantTrainingPlans)
                        {
                            string trainingPlanSpecialtyId = css.SpecialtyId;

                            if (user.SpecialtyId != trainingPlanSpecialtyId)
                            {
                                specialtyMismatch = true;
                                result.FailedCount++;
                                result.Errors.Add($"Error at row {row}: Trainee '{userId}' specialty ({user.SpecialtyId}) does not match with the Training Plan's specialty ({trainingPlanSpecialtyId}).");
                                break;  // Stop checking other training plans for this row
                            }
                        }

                        if (specialtyMismatch) continue;

                        string notes = worksheet.Cells[row, 2].Text ?? "";

                        lastIdNumber++;
                        string traineeAssignId = $"TA{lastIdNumber:D5}";

                        var traineeAssign = new TraineeAssign
                        {
                            TraineeAssignId = traineeAssignId,
                            TraineeId = userId,
                            CourseSubjectSpecialtyId = cssId,
                            AssignDate = DateTime.UtcNow,
                            RequestStatus = RequestStatus.Pending,
                            AssignByUserId = importedByUserId,
                            ApproveByUserId = null,
                            ApprovalDate = null,
                            Notes = notes
                        };

                        if (user != null)
                        {
                            user.IsAssign = true;
                            await _unitOfWork.UserRepository.UpdateAsync(user);
                            await _unitOfWork.SaveChangesAsync();
                        }
                        traineeAssignments.Add(traineeAssign);
                        existingAssignmentPairs.Add((userId, cssId));
                        processedUserIds.Add(userId);
                        result.SuccessCount++;
                    }

                    if (result.FailedCount > 0)
                    {
                        result.SuccessCount = 0;
                        return result;
                    }

                    await _unitOfWork.ExecuteWithStrategyAsync(async () =>
                    {
                        await _unitOfWork.BeginTransactionAsync();
                        try
                        {
                            var requestService = new RequestService(_unitOfWork, _mapper, _notificationService, _userRepository, _candidateRepository);
                            var requestDto = new RequestDTO
                            {
                                RequestType = RequestType.AssignTrainee,
                                Description = $"Request to confirm {result.SuccessCount} trainee assignments imported.",
                                Notes = "Trainee assignments pending approval"
                            };

                            var importRequest = await requestService.CreateRequestAsync(requestDto, importedByUserId);

                            foreach (var assignment in traineeAssignments)
                            {
                                assignment.RequestId = importRequest.RequestId;
                            }

                            await _unitOfWork.TraineeAssignRepository.AddRangeAsync(traineeAssignments);
                            await _unitOfWork.SaveChangesAsync();
                            await _unitOfWork.CommitTransactionAsync();
                        }
                        catch (Exception ex)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            result.Errors.Add($"Error saving data: {ex.Message}");
                            throw;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"General error: {ex.Message}");
                result.FailedCount = result.TotalRecords;
                result.SuccessCount = 0;
            }

            return result;
        }
        #endregion


        #region Get Trainees by SubjectId
        public async Task<List<TraineeAssignModel>> GetTraineesBySubjectIdAsync(string subjectId)
        {
            if (string.IsNullOrWhiteSpace(subjectId))
                throw new ArgumentException("Subject ID cannot be null or empty.");

            var cssList = await _unitOfWork.CourseSubjectSpecialtyRepository.GetAllAsync(
                css => css.SubjectId == subjectId,
                css => css.Trainees);
            var assignments = cssList.SelectMany(css => css.Trainees).ToList();

            if (!assignments.Any())
                throw new KeyNotFoundException($"No trainee assignments found for Subject with ID '{subjectId}'.");

            return _mapper.Map<List<TraineeAssignModel>>(assignments);
        }
        #endregion

        #region Get Trainees by RequestId
        public async Task<List<TraineeAssignModel>> GetTraineesByRequestIdAsync(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("Request ID cannot be null or empty.");

            var assignments = await _traineeAssignRepository.GetTraineeAssignmentsByRequestIdAsync(requestId);

            if (!assignments.Any())
                throw new KeyNotFoundException($"No trainee assignments found for Request with ID '{requestId}'.");

            return _mapper.Map<List<TraineeAssignModel>>(assignments);
        }
        #endregion

        #region Helper Methods
        private async Task<string> GetLastTraineeAssignIdAsync()
        {
            var traineeAssigns = await _unitOfWork.TraineeAssignRepository.GetAllAsync();

            if (!traineeAssigns.Any())
                return null;

            return traineeAssigns
                .Select(ta => ta.TraineeAssignId)
                .OrderByDescending(id =>
                {
                    string numericPart = new string(id.Where(char.IsDigit).ToArray());
                    if (int.TryParse(numericPart, out int number))
                        return number;
                    return 0;
                })
                .FirstOrDefault();
        }

        private bool IsRowEmpty(ExcelWorksheet worksheet, int row)
        {
            int totalColumns = worksheet.Dimension.Columns;
            for (int col = 1; col <= totalColumns; col++)
            {
                if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].GetValue<string>()))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
