using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OCMS_BOs;
using OCMS_BOs.Entities;
using System;
using System.Threading.Tasks;

namespace OCMS_Repositories
{
    public class UnitOfWork : IDisposable
    {
        private readonly OCMSDbContext _context;
        private IDbContextTransaction _transaction;
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
        private GenericRepository<Notification> _notificationRepository;
        private GenericRepository<ExternalCertificate> _externalCertificateRepository;
        private GenericRepository<InstructorAssignment> _instructorAssignmentRepository;
        private GenericRepository<CertificateTemplate> _certificateTemplateRepository;
        private GenericRepository<Grade> _gradeRepository;
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

        public GenericRepository<Notification> NotificationRepository
        {
            get => _notificationRepository ??= new GenericRepository<Notification>(_context);
        }

        public GenericRepository<TrainingSchedule> TrainingScheduleRepository
        {
            get => _trainingScheduleRepository ??= new GenericRepository<TrainingSchedule>(_context);
        }

        public GenericRepository<InstructorAssignment> InstructorAssignmentRepository
        {
            get => _instructorAssignmentRepository ??= new GenericRepository<InstructorAssignment>(_context);
        }

        public GenericRepository<TraineeAssign> TraineeAssignRepository
        {
            get => _traineeAssignRepository ??= new GenericRepository<TraineeAssign>(_context);
        }    
        
        public GenericRepository<CertificateTemplate> CertificateTemplateRepository
        {
            get => _certificateTemplateRepository ??= new GenericRepository<CertificateTemplate>(_context);
        }

        public GenericRepository<ExternalCertificate> ExternalCertificateRepository
        {
            get => _externalCertificateRepository ??= new GenericRepository<ExternalCertificate>(_context);
        }
        public GenericRepository<Grade> GradeRepository
        {
            get =>_gradeRepository ??= new GenericRepository<Grade>(_context);
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
        public async Task ExecuteWithStrategyAsync(Func<Task> operation)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(operation);
        }
    }
}