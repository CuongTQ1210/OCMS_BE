using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class InstructorAssignmentService : IInstructorAssignmentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;

        public InstructorAssignmentService(UnitOfWork unitOfWork, IMapper mapper, IRequestService requestService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
        }

        public async Task<IEnumerable<InstructorAssignmentModel>> GetAllInstructorAssignmentsAsync()
        {
            var assignments = await _unitOfWork.InstructorAssignmentRepository.GetAllAsync(
                a => a.Subject,
                a => a.Instructor
            );
            return _mapper.Map<IEnumerable<InstructorAssignmentModel>>(assignments);
        }

        public async Task<InstructorAssignmentModel> GetInstructorAssignmentByIdAsync(string assignmentId)
        {
            if (string.IsNullOrEmpty(assignmentId))
                throw new ArgumentException("Assignment ID cannot be null or empty.", nameof(assignmentId));

            var assignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.AssignmentId == assignmentId,
                a => a.Subject,
                a => a.Instructor
            );
            if (assignment == null)
                throw new KeyNotFoundException($"Instructor assignment with ID {assignmentId} not found.");

            return _mapper.Map<InstructorAssignmentModel>(assignment);
        }

        public async Task<InstructorAssignmentModel> CreateInstructorAssignmentAsync(InstructorAssignmentDTO dto, string assignByUserId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrEmpty(assignByUserId))
                throw new ArgumentException("AssignBy user ID cannot be null or empty.", nameof(assignByUserId));

            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectId);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectId} does not exist.");

            var instructorExists = await _unitOfWork.UserRepository.ExistsAsync(i => i.UserId == dto.InstructorId);
            if (!instructorExists)
                throw new ArgumentException($"Instructor with ID {dto.InstructorId} does not exist.");

            var userExists = await _unitOfWork.UserRepository.ExistsAsync(u => u.UserId == assignByUserId);
            if (!userExists)
                throw new ArgumentException($"User with ID {assignByUserId} does not exist.");

            string assignmentId;
            do
            {
                assignmentId = GenerateAssignmentId();
            } while (await _unitOfWork.InstructorAssignmentRepository.ExistsAsync(a => a.AssignmentId == assignmentId));

            var assignment = _mapper.Map<InstructorAssignment>(dto);
            assignment.AssignmentId = assignmentId;
            assignment.SubjectId = dto.SubjectId;
            assignment.InstructorId = dto.InstructorId;
            assignment.AssignByUserId = assignByUserId;
            assignment.AssignDate = DateTime.UtcNow;
            assignment.RequestStatus = RequestStatus.Pending;

            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == dto.InstructorId);
            if (user != null)
            {
                user.IsAssign = true;
                await _unitOfWork.UserRepository.UpdateAsync(user);
            }

            await _unitOfWork.InstructorAssignmentRepository.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            // Create approval request for the instructor assignment
            var requestDto = new RequestDTO
            {
                RequestEntityId = assignmentId,
                RequestType = RequestType.AssignInstructor,
                Description = $"Request to approve instructor assignment for subject {dto.SubjectId}",
                Notes = $"Assignment details:\n" +
                       $"Instructor ID: {dto.InstructorId}\n" +
                       $"Subject ID: {dto.SubjectId}\n" +
                       $"Assignment Date: {assignment.AssignDate}"
            };
            await _requestService.CreateRequestAsync(requestDto, assignByUserId);

            var createdAssignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.AssignmentId == assignmentId,
                a => a.Subject,
                a => a.Instructor
            );
            return _mapper.Map<InstructorAssignmentModel>(createdAssignment);
        }

        public async Task<InstructorAssignmentModel> UpdateInstructorAssignmentAsync(string assignmentId, InstructorAssignmentDTO dto)
        {
            if (string.IsNullOrEmpty(assignmentId))
                throw new ArgumentException("Assignment ID cannot be null or empty.", nameof(assignmentId));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var assignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new KeyNotFoundException($"Instructor assignment with ID {assignmentId} not found.");

            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectId);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectId} does not exist.");

            var instructorExists = await _unitOfWork.UserRepository.ExistsAsync(i => i.UserId == dto.InstructorId);
            if (!instructorExists)
                throw new ArgumentException($"Instructor with ID {dto.InstructorId} does not exist.");

            if (assignment.RequestStatus == RequestStatus.Approved)
            {
                throw new InvalidOperationException("Assignment has been approved. Create a request to update if needed.");
            }

            _mapper.Map(dto, assignment);
            assignment.SubjectId = dto.SubjectId;
            assignment.InstructorId = dto.InstructorId;

            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == dto.InstructorId);
            if (user != null)
            {
                user.IsAssign = true;
                await _unitOfWork.UserRepository.UpdateAsync(user);
            }

            await _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            var updatedAssignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.AssignmentId == assignmentId,
                a => a.Subject,
                a => a.Instructor
            );
            return _mapper.Map<InstructorAssignmentModel>(updatedAssignment);
        }

        public async Task<bool> DeleteInstructorAssignmentAsync(string assignmentId)
        {
            if (string.IsNullOrEmpty(assignmentId))
                throw new ArgumentException("Assignment ID cannot be null or empty.", nameof(assignmentId));

            var assignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new KeyNotFoundException($"Instructor assignment with ID {assignmentId} not found.");

            if (assignment.RequestStatus == RequestStatus.Approved)
            {
                throw new InvalidOperationException("Assignment has been approved. Create a request to delete if needed.");
            }

            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == assignment.InstructorId);
            if (user != null)
            {
                user.IsAssign = false;
                await _unitOfWork.UserRepository.UpdateAsync(user);
            }

            await _unitOfWork.InstructorAssignmentRepository.DeleteAsync(assignmentId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private string GenerateAssignmentId()
        {
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"ASG-{guidPart}";
        }
    }
}
