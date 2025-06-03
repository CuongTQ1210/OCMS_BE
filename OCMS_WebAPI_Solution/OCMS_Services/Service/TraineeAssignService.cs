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
        private readonly IGradeService _gradeService;

        public TraineeAssignService(
            UnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IUserRepository userRepository,
            ICandidateRepository candidateRepository,
            ICourseRepository courseRepository,
            ITraineeAssignRepository traineeAssignRepository,
            IGradeService gradeService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
            _courseRepository = courseRepository;
            _traineeAssignRepository = traineeAssignRepository;
            _gradeService = gradeService;
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

            // Validate ClassId
            var classSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(dto.ClassSubjectId);
            if (classSubject == null)
                throw new Exception($"ClassSubject with ID {dto.ClassSubjectId} not found.");

            

            // Find the specialty for this subject
            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.GetByIdAsync(classSubject.SubjectSpecialtyId);
             
                
            // Check if trainee's specialty matches the subject's specialty
            if (trainee.SpecialtyId != subjectSpecialty.SpecialtyId)
            {
                throw new Exception($"Trainee's specialty ({trainee.SpecialtyId}) does not match with the Subject's specialty ({subjectSpecialty.SpecialtyId}).");
            }

            // Check if the trainee is already assigned to this Class (only if changing the trainee or Class)
            if (dto.TraineeId != existingAssignment.TraineeId || dto.ClassSubjectId != existingAssignment.ClassSubjectId)
            {
                var existingAssignmentCheck = await _unitOfWork.TraineeAssignRepository
                    .FindAsync(ta => ta.TraineeId == dto.TraineeId && ta.ClassSubjectId == dto.ClassSubjectId && ta.TraineeAssignId != id);
                if (existingAssignmentCheck.Any())
                    throw new Exception($"Trainee {dto.TraineeId} is already assigned to ClassSubject {dto.ClassSubjectId}.");
            }

            // Update the IsAssign status of the user if needed
            if (trainee != null && !trainee.IsAssign)
            {
                trainee.IsAssign = true;
                await _unitOfWork.UserRepository.UpdateAsync(trainee);
            }

            // Update the assignment
            existingAssignment.TraineeId = dto.TraineeId;
            existingAssignment.ClassSubjectId = dto.ClassSubjectId;
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
            var classSubjectIds = assignments.Select(a => a.ClassSubjectId).Distinct().ToList();
            var classes = await _unitOfWork.ClassSubjectRepository.GetAllAsync(c => classSubjectIds.Contains(c.ClassSubjectId));
            var courseIds = classes.Select(c => c.ClassId).Distinct().ToList();
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

            // Validate ClassId
            var classSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(dto.ClassSubjectId);
            if (classSubject == null)
            {
                throw new Exception($"ClassSubject with ID {dto.ClassSubjectId} not found.");
            }

            // Find the specialty for this subject
            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.GetByIdAsync(classSubject.SubjectSpecialtyId);
            
            // Check if trainee's specialty matches the subject's specialty
            if (trainee.SpecialtyId != subjectSpecialty.SpecialtyId)
            {
                throw new Exception($"Trainee's specialty ({trainee.SpecialtyId}) does not match with the Subject's specialty ({subjectSpecialty.SpecialtyId}).");
            }

            // Check if the trainee is already assigned to this Class
            var existingAssignment = await _unitOfWork.TraineeAssignRepository
                .FindAsync(ta => ta.TraineeId == dto.TraineeId && ta.ClassSubjectId == dto.ClassSubjectId);
            if (existingAssignment.Any())
            {
                throw new Exception($"Trainee {dto.TraineeId} is already assigned to ClassSubject {dto.ClassSubjectId}.");
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
                RequestDate = DateTime.Now,
                Status = RequestStatus.Pending,
                Description = $"Assign trainee {dto.TraineeId} to ClassSubject {dto.ClassSubjectId}.",
                Notes = $"Request to assign Trainee {dto.TraineeId} to ClassSubject {dto.ClassSubjectId}."
            };

            //Create TraineeAssign object with RequestId
            var traineeAssign = new TraineeAssign
            {
                TraineeAssignId = newTraineeAssignId,
                TraineeId = dto.TraineeId,
                ClassSubjectId = dto.ClassSubjectId,
                ClassSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(dto.ClassSubjectId),
                RequestId = newRequest.RequestId,
                AssignByUserId = createdByUserId,
                RequestStatus = RequestStatus.Pending,
                AssignDate = DateTime.Now,
                ApprovalDate = null,
                ApproveByUserId = null,
                Notes = dto.Notes ?? ""
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
                var existingCss = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                    css => css.Class);

                // Now, separately get all coursesIds
                var courseIds = existingCss.Select(css => css.ClassId).Distinct().ToList();

                // Remove references to TrainingPlan which doesn't exist in the current schema
                var existingCssIds = existingCss.Select(css => css.ClassSubjectId).ToList();
                var cssDict = existingCss.ToDictionary(css => css.ClassSubjectId, css => css);

                var existingUsers = await _unitOfWork.UserRepository.GetAllAsync();
                var userDict = existingUsers.ToDictionary(u => u.UserId, u => u);

                var existingAssignments = await _unitOfWork.TraineeAssignRepository.GetAllAsync();
                var existingAssignmentPairs = existingAssignments
                    .Select(ta => new { ta.TraineeId, ta.ClassSubjectId })
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

                    // Read ClassSubjectId from cell B1
                    var classSubjectId = worksheet.Cells[1, 2].GetValue<string>();
                    if (string.IsNullOrEmpty(classSubjectId))
                    {
                        result.Errors.Add("Invalid or missing ClassSubjectId in cell B1.");
                        return result;
                    }

                    var classSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(classSubjectId);
                    if (classSubject == null)
                    {
                        result.Errors.Add($"ClassSubject with ID '{classSubjectId}' not found.");
                        return result;
                    }


                    // Prepare for import
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

                    // Count non-empty trainee rows (starting from row 3)
                    int traineeRowCount = 0;
                    for (int row = 3; row <= rowCount; row++)
                    {
                        if (!IsRowEmpty(worksheet, row))
                            traineeRowCount++;
                    }

                    // Validate trainee count
                    if (traineeRowCount < 5)
                    {
                        result.Errors.Add("The minimum number of trainees per import is 5.");
                        result.FailedCount = traineeRowCount;
                        return result;
                    }
                    if (traineeRowCount > 35)
                    {
                        result.Errors.Add("The maximum number of trainees per import is 35.");
                        result.FailedCount = traineeRowCount;
                        return result;
                    }
                    result.TotalRecords = traineeRowCount;

                    // Start from row 3
                    for (int row = 3; row <= rowCount; row++)
                    {
                        if (IsRowEmpty(worksheet, row)) continue;

                        string userId = worksheet.Cells[row, 1].GetValue<string>(); // Column A
                        string notes = worksheet.Cells[row, 2].Text ?? ""; // Column B

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

                        // Validate ClassSubject
                        var classSubjectFromDict = cssDict[classSubjectId];
                        if (classSubjectFromDict == null)
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: ClassSubject with ID '{classSubjectId}' not found.");
                            continue;
                        }

                        // Find the specialty for this subject
                        var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.GetByIdAsync(classSubjectFromDict.SubjectSpecialtyId);
                        if (subjectSpecialty == null)
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: SubjectSpecialty with ID '{classSubjectFromDict.SubjectSpecialtyId}' not found.");
                            continue;
                        }

                        // Check if trainee's specialty matches the subject's specialty
                        if (user.SpecialtyId != subjectSpecialty.SpecialtyId)
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: Trainee's specialty ({user.SpecialtyId}) does not match with the Subject's specialty ({subjectSpecialty.SpecialtyId}).");
                            continue;
                        }

                        // Check if the trainee is already assigned to this ClassSubject
                        if (existingAssignmentPairs.Contains(new { TraineeId = userId, ClassSubjectId = classSubjectId }))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: Trainee '{userId}' is already assigned to ClassSubject '{classSubjectId}'.");
                            continue;
                        }

                        lastIdNumber++;
                        string traineeAssignId = $"TA{lastIdNumber:D5}";

                        var traineeAssign = new TraineeAssign
                        {
                            TraineeAssignId = traineeAssignId,
                            TraineeId = userId,
                            ClassSubjectId = classSubjectId,
                            ClassSubject = await _unitOfWork.ClassSubjectRepository.GetByIdAsync(classSubjectId),
                            AssignDate = DateTime.Now,
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
                        existingAssignmentPairs.Add(new { TraineeId = userId, ClassSubjectId = classSubjectId.ToString() });
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

            var classSubjects = await _unitOfWork.ClassSubjectRepository.GetAllAsync(
                cs => cs.SubjectSpecialty.SubjectId == subjectId);

            var classSubjectIds = classSubjects.Select(cs => cs.ClassSubjectId).ToList();
            
            var assignments = await _unitOfWork.TraineeAssignRepository.GetAllAsync(
                ta => classSubjectIds.Contains(ta.ClassSubjectId));

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
