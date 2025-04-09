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

        public InstructorAssignmentService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Retrieves all instructor assignments with related Subject and Instructor data.
        /// </summary>
        public async Task<IEnumerable<InstructorAssignmentModel>> GetAllInstructorAssignmentsAsync()
        {
            var assignments = await _unitOfWork.InstructorAssignmentRepository.GetAllAsync(
                a => a.Subject,
                a => a.Instructor
            );
            return _mapper.Map<IEnumerable<InstructorAssignmentModel>>(assignments);
        }

        /// <summary>
        /// Retrieves an instructor assignment by its ID, including related data.
        /// </summary>
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

        /// <summary>
        /// Creates a new instructor assignment with the provided DTO and user ID.
        /// </summary>
        public async Task<InstructorAssignmentModel> CreateInstructorAssignmentAsync(InstructorAssignmentDTO dto, string assignByUserId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrEmpty(assignByUserId))
                throw new ArgumentException("AssignBy user ID cannot be null or empty.", nameof(assignByUserId));

            // Validate SubjectID
            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectId);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectId} does not exist.");

            // Validate InstructorID
            var instructorExists = await _unitOfWork.UserRepository.ExistsAsync(i => i.UserId == dto.InstructorId);
            if (!instructorExists)
                throw new ArgumentException($"Instructor with ID {dto.InstructorId} does not exist.");

            // Validate AssignByUserId
            var userExists = await _unitOfWork.UserRepository.ExistsAsync(u => u.UserId == assignByUserId);
            if (!userExists)
                throw new ArgumentException($"User with ID {assignByUserId} does not exist.");

            // Generate AssignmentId in the format ASG-XXXXXX
            string assignmentId;
            do
            {
                assignmentId = GenerateAssignmentId();
            } while (await _unitOfWork.InstructorAssignmentRepository.ExistsAsync(a => a.AssignmentId == assignmentId));

            // Map DTO to entity
            var assignment = _mapper.Map<InstructorAssignment>(dto);
            assignment.AssignmentId = assignmentId;
            assignment.AssignByUserId = assignByUserId;
            assignment.AssignDate = DateTime.UtcNow;
            assignment.RequestStatus = RequestStatus.Pending; // Assuming RequestStatus is an enum
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == dto.InstructorId);
            if (user != null)
            {
                user.IsAssign = true;
                _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            await _unitOfWork.InstructorAssignmentRepository.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            // Fetch with related data
            var createdAssignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.AssignmentId == assignmentId,
                a => a.Subject,
                a => a.Instructor
            );
            return _mapper.Map<InstructorAssignmentModel>(createdAssignment);
        }

        /// <summary>
        /// Updates an existing instructor assignment with the provided DTO.
        /// </summary>
        public async Task<InstructorAssignmentModel> UpdateInstructorAssignmentAsync(string assignmentId, InstructorAssignmentDTO dto)
        {
            if (string.IsNullOrEmpty(assignmentId))
                throw new ArgumentException("Assignment ID cannot be null or empty.", nameof(assignmentId));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var assignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new KeyNotFoundException($"Instructor assignment with ID {assignmentId} not found.");

            // Validate SubjectID
            var subjectExists = await _unitOfWork.SubjectRepository.ExistsAsync(s => s.SubjectId == dto.SubjectId);
            if (!subjectExists)
                throw new ArgumentException($"Subject with ID {dto.SubjectId} does not exist.");

            // Validate InstructorID
            var instructorExists = await _unitOfWork.InstructorAssignmentRepository.ExistsAsync(i => i.InstructorId == dto.InstructorId);
            if (!instructorExists)
                throw new ArgumentException($"Instructor with ID {dto.InstructorId} does not exist.");

            // Map DTO to existing entity
            _mapper.Map(dto, assignment);
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == dto.InstructorId);
            if (user != null)
            {
                user.IsAssign = true;
                _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            _unitOfWork.InstructorAssignmentRepository.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            // Fetch with related data
            var updatedAssignment = await _unitOfWork.InstructorAssignmentRepository.GetAsync(
                a => a.AssignmentId == assignmentId,
                a => a.Subject,
                a => a.Instructor
            );
            return _mapper.Map<InstructorAssignmentModel>(updatedAssignment);
        }

        /// <summary>
        /// Deletes an instructor assignment by its ID.
        /// </summary>
        public async Task<bool> DeleteInstructorAssignmentAsync(string assignmentId)
        {
            if (string.IsNullOrEmpty(assignmentId))
                throw new ArgumentException("Assignment ID cannot be null or empty.", nameof(assignmentId));

            var assignment = await _unitOfWork.InstructorAssignmentRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new KeyNotFoundException($"Instructor assignment with ID {assignmentId} not found.");
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.UserId == assignment.InstructorId);
            if (user != null)
            {
                user.IsAssign = false;
                _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }
            _unitOfWork.InstructorAssignmentRepository.DeleteAsync(assignmentId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Generates an AssignmentId in the format ASG-XXXXXX where XXXXXX is a random 6-digit number.
        /// </summary>
        private string GenerateAssignmentId()
        {
            string guidPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); // Get first 6 characters
            return $"ASG-{guidPart}";
        }
    }
}
