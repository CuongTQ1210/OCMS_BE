﻿using OCMS_BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetDepartmentsBySpecialtyIdAsync(string specialtyId);
    }
}
