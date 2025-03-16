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
        private GenericRepository<ExternalCertificate> _externalCertificateRepository;

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

        public GenericRepository<ExternalCertificate> ExternalCertificateRepository
        {
            get => _externalCertificateRepository ??= new GenericRepository<ExternalCertificate>(_context);
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
    }
}