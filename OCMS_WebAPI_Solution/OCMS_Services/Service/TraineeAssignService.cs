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

        public TraineeAssignService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            ICourseRepository courseRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
            _courseRepository = courseRepository;
        }
        #region Get All Trainee Assignments
        public async Task<IEnumerable<TraineeAssignModel>> GetAllTraineeAssignmentsAsync()
        {
            var assignments = await _unitOfWork.TraineeAssignRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TraineeAssignModel>>(assignments);
        }

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
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == dto.TraineeId);
            if (user != null)
            {
                user.IsAssign = true;
                _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            // Ensure update is allowed only if status is "Pending" or "Rejected"
            if (existingAssignment.RequestStatus.ToString() != "Pending" && existingAssignment.RequestStatus.ToString() != "Rejected")
                throw new InvalidOperationException($"Cannot update trainee assignment because its status is '{existingAssignment.RequestStatus.ToString()}'. Only 'Pending' or 'Rejected' can be updated.");

            existingAssignment.TraineeId = dto.TraineeId;
            existingAssignment.CourseId = dto.CourseId;
            existingAssignment.Notes = dto.Notes;

            _unitOfWork.TraineeAssignRepository.UpdateAsync(existingAssignment);
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
                return (false, $"Cannot delete trainee assignment because its status is '{assignment.RequestStatus.ToString()}'. Only 'Pending' or 'Rejected' can be deleted.");

            _unitOfWork.TraineeAssignRepository.DeleteAsync(id);
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == assignment.TraineeId);
            if (user != null)
            {
                user.IsAssign = false;
                _unitOfWork.UserRepository.UpdateAsync(user);
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
            var courseIds = assignments.Select(a => a.CourseId).Distinct().ToList();

            var courses = await _unitOfWork.CourseRepository.GetAllAsync(c => courseIds.Contains(c.CourseId));
            return _mapper.Map<IEnumerable<CourseModel>>(courses);
        }
        #endregion

        #region Generate Assignment ID
        private string GenerateAssignmentId()
        {
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"TAS-{guidPart}";
        }
        #endregion
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

            // Validate CourseId
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseId);
            if (course == null)
            {
                throw new Exception($"Course with ID {dto.CourseId} not found.");
            }

            // Check if the trainee is already assigned to this course
            var existingAssignment = await _unitOfWork.TraineeAssignRepository
                .FindAsync(ta => ta.TraineeId == dto.TraineeId && ta.CourseId == dto.CourseId);
            if (existingAssignment.Any())
            {
                throw new Exception($"Trainee {dto.TraineeId} is already assigned to Course {dto.CourseId}.");
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

            // ✅ Create a new Request for approval
            var newRequest = new Request
            {
                RequestId = $"REQ-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                RequestType = RequestType.AddTraineeAssign, // Assuming an enum exists
                RequestUserId=createdByUserId,
                RequestDate=DateTime.UtcNow,
                Status= RequestStatus.Pending,
                Description= $"Assign trainee {dto.TraineeId} to Course {dto.CourseId}.",
                Notes = $"Request to assign Trainee {dto.TraineeId} to Course {dto.CourseId}.",
            };

            // ✅ Create TraineeAssign object with RequestId
            var traineeAssign = new TraineeAssign
            {
                TraineeAssignId = newTraineeAssignId,
                TraineeId = dto.TraineeId,
                CourseId = dto.CourseId,
                RequestId = newRequest.RequestId, // Link to the request
                AssignByUserId= createdByUserId,
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
                _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            // ✅ Save both Request & TraineeAssign in a single transaction
            await _unitOfWork.RequestRepository.AddAsync(newRequest);
            await _unitOfWork.TraineeAssignRepository.AddAsync(traineeAssign);
            await _unitOfWork.SaveChangesAsync();

            // ✅ Return TraineeAssignModel
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
                // Fetch existing data for validation
                var existingCourses = await _unitOfWork.CourseRepository.GetAllAsync();
                var existingCourseIds = existingCourses.Select(c => c.CourseId).ToList();

                var existingUsers = await _unitOfWork.UserRepository.GetAllAsync();
                var userDict = existingUsers.ToDictionary(u => u.UserId, u => u);

                var existingAssignments = await _unitOfWork.TraineeAssignRepository.GetAllAsync();
                var existingAssignmentPairs = existingAssignments
                    .Select(ta => (ta.TraineeId, ta.CourseId))
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

                    // Read CourseId from cell B1
                    string courseId = worksheet.Cells[1, 2].GetValue<string>(); // B1 (row 1, column 2)
                    if (string.IsNullOrEmpty(courseId) || !existingCourseIds.Contains(courseId))
                    {
                        result.Errors.Add($"Invalid or missing CourseId '{courseId}' in cell B1.");
                        return result;
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

                        // Read UserId from column A (1) - A3, A4, A视角

                        string userId = worksheet.Cells[row, 1].GetValue<string>(); // Column A (A3, A4, A5, ...)
                                                                                    // Log the UserId being read
                        //result.Errors.Add($"Debug: Row {row} - UserId (A{row}): '{userId}'");
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
                        if (user.UserId == null || user.RoleId != 7)
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: User with ID '{userId}' is not a Trainee. Role: {(user.RoleId != null ? user.RoleId.ToString() : "None")}.");
                            continue;
                        }

                        if (existingAssignmentPairs.Contains((userId, courseId)))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: Trainee '{userId}' is already assigned to Course '{courseId}'.");
                            continue;
                        }

                        // Read Notes from column B (2) - B3, B4, B5, ...
                        string notes = worksheet.Cells[row, 2].Text ?? "";
                        // Log the Notes being read
                        result.Errors.Add($"Debug: Row {row} - Notes (B{row}): '{notes}'");

                        lastIdNumber++;
                        string traineeAssignId = $"TA{lastIdNumber:D5}";

                        var traineeAssign = new TraineeAssign
                        {
                            TraineeAssignId = traineeAssignId,
                            TraineeId = userId,
                            CourseId = courseId,
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
                            _unitOfWork.UserRepository.UpdateAsync(user);
                            await _unitOfWork.SaveChangesAsync();
                        }
                        traineeAssignments.Add(traineeAssign);
                        existingAssignmentPairs.Add((userId, courseId));
                        processedUserIds.Add(userId);
                        result.SuccessCount++;
                    }

                    if (result.FailedCount > 0)
                    {
                        result.SuccessCount = 0;
                        return result;
                    }

                    // Save to database
                    await _unitOfWork.ExecuteWithStrategyAsync(async () =>
                    {
                        await _unitOfWork.BeginTransactionAsync();
                        try
                        {
                            // Create a single request for all assignments
                            var requestService = new RequestService(_unitOfWork, _mapper, _notificationService, _userRepository, _candidateRepository);
                            var requestDto = new RequestDTO
                            {
                                RequestType = RequestType.AssignTrainee,
                                Description = $"Request to confirm {result.SuccessCount} trainee assignments imported.",
                                Notes = "Trainee assignments pending approval"
                            };

                            // Generate a single Request ID
                            var importRequest = await requestService.CreateRequestAsync(requestDto, importedByUserId);

                            // Attach the same Request ID to all TraineeAssign records
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
