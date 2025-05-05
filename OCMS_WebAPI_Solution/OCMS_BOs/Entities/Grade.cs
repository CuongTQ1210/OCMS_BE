﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.Entities
{
    public class Grade
    {
        [Key]
        public string GradeId { get; set; }

        [ForeignKey("TraineeAssign")]
        public string TraineeAssignID { get; set; }
        public TraineeAssign TraineeAssign { get; set; }
        public double ParticipantScore { get; set; }
        public double AssignmentScore    { get; set; }
        public double FinalExamScore { get; set; }
        public double? FinalResitScore { get; set; }
        public double TotalScore { get; set; }
        public GradeStatus gradeStatus { get; set; }
        public string Remarks { get; set; }

        [ForeignKey("GradeUser")]
        public string GradedByInstructorId { get; set; }
        public User GradedByInstructor { get; set; }

        public DateTime EvaluationDate { get; set; }= DateTime.Now;

        public DateTime UpdateDate { get; set; }=DateTime.Now;

    }
}
