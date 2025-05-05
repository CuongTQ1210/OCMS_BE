using Microsoft.EntityFrameworkCore;
using OCMS_BOs;
using OCMS_BOs.Entities;
using OCMS_BOs.ViewModel;
using OCMS_Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.Repository
{
    public class TraineeAssignRepository : GenericRepository<TraineeAssign>, ITraineeAssignRepository
    {
        private readonly OCMSDbContext _context;
        public TraineeAssignRepository(OCMSDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.TrainingPlans.AnyAsync(tp => tp.PlanId == id);
        }

        public async Task<TraineeAssign> GetTraineeAssignmentAsync(string courseSubjectId, string traineeId)
        {
            return await _context.TraineeAssignments
                .Include(ta => ta.CourseSubjectSpecialty)
                .Include(ta => ta.Trainee)
                .FirstOrDefaultAsync(ta => ta.CourseSubjectSpecialtyId == courseSubjectId && ta.TraineeId == traineeId);
        }

        public async Task<IEnumerable<TraineeAssign>> GetTraineeAssignmentsByCourseSubjectIdAsync(string courseSubjectId)
        {
            return await _context.TraineeAssignments
                .Include(ta => ta.Trainee)
                .Where(ta => ta.CourseSubjectSpecialtyId == courseSubjectId && ta.RequestStatus == RequestStatus.Approved)
                .ToListAsync();
        }

        public async Task<IEnumerable<TraineeAssign>> GetTraineeAssignmentsByCourseIdAsync(string courseId)
        {
            return await _context.TraineeAssignments
                .Include(ta => ta.Trainee)
                .Where(ta => ta.CourseSubjectSpecialty.CourseId == courseId && ta.RequestStatus == RequestStatus.Approved)
                .ToListAsync();
        }
        public async Task<List<TraineeAssignModel>> GetTraineeAssignmentsByRequestIdAsync(string requestId)
        {
            return await _context.TraineeAssignments
                .Where(ta => ta.RequestId == requestId)
                .Select(ta => new TraineeAssignModel
                {
                    TraineeAssignId = ta.TraineeAssignId,
                    TraineeId = ta.TraineeId,
                    CourseSubjectSpecialtyId = ta.CourseSubjectSpecialtyId,
                    Notes = ta.Notes,
                    RequestStatus = ta.RequestStatus.ToString(),
                    AssignByUserId = ta.AssignByUserId,
                    AssignDate = ta.AssignDate,
                    ApproveByUserId = ta.ApproveByUserId,
                    ApprovalDate = ta.ApprovalDate,
                    RequestId = ta.RequestId
                })
                .ToListAsync();
        }


        public async Task<List<TraineeAssignModel>> GetTraineeAssignsByCourseSubjectIdAsync(string courseSubjectId)
        {
            var courseSubject = await _context.CourseSubjectSpecialties.FindAsync(courseSubjectId);
            if (courseSubject == null)
                return new List<TraineeAssignModel>();

            return await _context.TraineeAssignments
                .Where(ta => ta.CourseSubjectSpecialtyId == courseSubject.CourseId)
                .Select(ta => new TraineeAssignModel
                {
                    TraineeAssignId = ta.TraineeAssignId,
                    TraineeId = ta.TraineeId,
                    CourseSubjectSpecialtyId = ta.CourseSubjectSpecialtyId,
                    Notes = ta.Notes,
                    RequestStatus = ta.RequestStatus.ToString(),
                    AssignByUserId = ta.AssignByUserId,
                    AssignDate = ta.AssignDate,
                    ApproveByUserId = ta.ApproveByUserId,
                    ApprovalDate = ta.ApprovalDate,
                    RequestId = ta.RequestId
                }).ToListAsync();
        }
    }
}
