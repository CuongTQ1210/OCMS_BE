﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class TrainingPlan
    {
        [Key]
        public string PlanId { get; set; }
        public string PlanName { get; set; }
        public string Desciption { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [ForeignKey("CreateUser")]
        public string CreateByUserId { get; set; }
        public User CreateByUser { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime ModifyDate { get; set; } = DateTime.Now;

        [ForeignKey("ApproveUser")]
        public string? ApproveByUserId { get; set; }
        public User? ApproveByUser { get; set; }
        public DateTime? ApproveDate { get; set; } = DateTime.Now;

        public TrainingPlanStatus TrainingPlanStatus { get; set; }
        public List<Course> Courses { get; set; }
    }
}
