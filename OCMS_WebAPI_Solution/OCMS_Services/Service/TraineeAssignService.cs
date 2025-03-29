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
                    result.TotalRecords = rowCount - 1;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        if (IsRowEmpty(worksheet, row)) continue;

                        string courseId = worksheet.Cells[row, 1].GetValue<string>();
                        if (string.IsNullOrEmpty(courseId) || !existingCourseIds.Contains(courseId))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: Invalid or missing CourseId '{courseId}'.");
                            continue;
                        }

                        string userId = worksheet.Cells[row, 2].GetValue<string>();
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
                        if (user.Role.RoleName != "Trainee")
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: User with ID '{userId}' is not a Trainee. Role: {user.Role}.");
                            continue;
                        }

                        if (existingAssignmentPairs.Contains((userId, courseId)))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: Trainee '{userId}' is already assigned to Course '{courseId}'.");
                            continue;
                        }

                        string notes = worksheet.Cells[row, 3].GetValue<string>();

                        lastIdNumber++;
                        string traineeAssignId = $"TA{lastIdNumber:D5}";

                        var traineeAssign = new TraineeAssign
                        {
                            TraineeAssignId = traineeAssignId,
                            TraineeId = userId,
                            CourseId = courseId,
                            AssignDate = DateTime.UtcNow,
                            RequestStatus = RequestStatus.Pending,
                            AssignByUserId= importedByUserId,
                            ApproveByUserId = null,
                            ApprovalDate = null,
                            Notes = notes
                        };

                        traineeAssignments.Add(traineeAssign);
                        existingAssignmentPairs.Add((userId, courseId));
                        processedUserIds.Add(userId);
                        result.SuccessCount++;
                    }

                    //if (processedUserIds.Count < 10)
                    //{
                    //    result.Errors.Add($"Import failed: The list must contain at least 10 unique users. Found {processedUserIds.Count} unique users.");
                    //    result.SuccessCount = 0;
                    //    result.FailedCount = result.TotalRecords;
                    //    return result;
                    //}

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
