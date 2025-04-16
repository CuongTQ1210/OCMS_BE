using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_Repositories;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class DepartmentService : IDepartmentService
    {

        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region GetAllDepartmentsAsync
        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
            return departments;
        }
        #endregion
    }
}
