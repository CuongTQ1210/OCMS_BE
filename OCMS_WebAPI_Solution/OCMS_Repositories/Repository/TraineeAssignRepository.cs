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

        //public async Task<bool> ExistsAsync(string id)
        //{
        //    return await _context.TrainingPlans.AnyAsync(tp => tp.PlanId == id);
        //}

        public async Task<TraineeAssign> GetTraineeAssignmentAsync(string courseId, string traineeId)
        {
            return await _context.TraineeAssignments
                .Include(ta => ta.ClassSubject)
                .Include(ta => ta.Trainee)
                .FirstOrDefaultAsync(ta => ta.ClassSubject.ClassId == courseId && ta.TraineeId == traineeId);
        }

        public async Task<IEnumerable<TraineeAssign>> GetTraineeAssignmentsByClassSubjectIdAsync(string classSubjectId)
        {
            return await _context.TraineeAssignments
                .Include(ta => ta.Trainee)
                .Where(ta => ta.ClassSubjectId == classSubjectId && ta.RequestStatus == RequestStatus.Approved)
                .ToListAsync();
        }

        public async Task<IEnumerable<TraineeAssign>> GetTraineeAssignmentsByCourseIdAsync(string courseId)
        {
            return await _context.TraineeAssignments
                .Include(ta => ta.Trainee)
                .Include(ta => ta.ClassSubject)
                .Where(ta => ta.ClassSubject.Class.CourseId == courseId && ta.RequestStatus == RequestStatus.Approved)
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
                    ClassSubjectId = ta.ClassSubjectId,
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


        public async Task<List<TraineeAssignModel>> GetTraineeAssignsByClassSubjectIdAsync(string classSubjectId)
        {
            var classSubject = await _context.ClassSubjects.FindAsync(classSubjectId);
            if (classSubject == null)
                return new List<TraineeAssignModel>();

            return await _context.TraineeAssignments
                .Where(ta => ta.ClassSubjectId == classSubjectId)
                .Select(ta => new TraineeAssignModel
                {
                    TraineeAssignId = ta.TraineeAssignId,
                    TraineeId = ta.TraineeId,
                    ClassSubjectId = ta.ClassSubjectId,
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
