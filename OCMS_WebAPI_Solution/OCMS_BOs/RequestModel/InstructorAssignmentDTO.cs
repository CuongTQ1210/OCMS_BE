﻿using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_BOs.RequestModel
{
    public class InstructorAssignmentDTO
    {
        public string CourseSubjectSpecialtyId { get; set; }
        public string InstructorId { get; set; }
        public string Notes { get; set; }
    }
}
