﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Repositories.IRepository
{
    public interface ITraineeAssignRepository
    {
        Task<bool> ExistsAsync(string id);
    }
}
