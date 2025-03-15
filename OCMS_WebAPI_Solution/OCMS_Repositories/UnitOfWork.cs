using OCMS_BOs;
using OCMS_BOs.Entities;
using System;
using System.Threading.Tasks;

namespace OCMS_Repositories
{
    public class UnitOfWork : IDisposable
    {
        private readonly OCMSDbContext _context;
        private GenericRepository<User> _userRepository;
        private GenericRepository<Course> _courseRepository;
        private GenericRepository<Department> _departmentRepository;
        private GenericRepository<Role> _roleRepository;
        private GenericRepository<Specialties> _specialtiesRepository;
        private GenericRepository<Candidate> _candidateRepository;
        private GenericRepository<TrainingPlan> _trainingplanRepository;
        private GenericRepository<Subject> _subjectRepository;
        private GenericRepository<TraineeAssign> _traineeAssignRepository;
        private GenericRepository<InstructorAssignment> _instructorAssignRepository;
        private GenericRepository<Request> _requestRepository;
        private GenericRepository<TrainingSchedule> _trainingScheduleRepository;
        private GenericRepository<CourseParticipant> _courseParticipantRepository;
        public UnitOfWork(OCMSDbContext context)
        {
            _context = context;
        }

        public GenericRepository<User> UserRepository
        {
            get => _userRepository ??= new GenericRepository<User>(_context);
        }

        public GenericRepository<Course> CourseRepository
        {
            get => _courseRepository ??= new GenericRepository<Course>(_context);
        }

        public GenericRepository<Department> DepartmentRepository
        {
            get => _departmentRepository ??= new GenericRepository<Department>(_context);
        }

        public GenericRepository<Role> RoleRepository
        {
            get => _roleRepository ??= new GenericRepository<Role>(_context);
        }

        public GenericRepository<Specialties> SpecialtyRepository
        {
            get => _specialtiesRepository ??= new GenericRepository<Specialties>(_context);
        }

        public GenericRepository<Candidate> CandidateRepository
        {
            get => _candidateRepository ??= new GenericRepository<Candidate>(_context);
        }
        public GenericRepository<TrainingPlan> TrainingPlanRepository
        {
            get=> _trainingplanRepository ??= new GenericRepository<TrainingPlan>(_context);
        }
        public GenericRepository<Subject> SubjectRepository
        {
            get => _subjectRepository ??= new GenericRepository<Subject>(_context);
        }
        public GenericRepository<Request> RequestRepository
        {
            get => _requestRepository ??= new GenericRepository<Request>(_context);
        }


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
