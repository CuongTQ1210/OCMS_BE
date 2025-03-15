using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SpecialtyService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SpecialtyModel> AddSpecialtyAsync(Specialties specialty)
        {
            var existingSpecialty = (await _unitOfWork.SpecialtyRepository.GetAllAsync()).FirstOrDefault(s => s.SpecialtyName == specialty.SpecialtyName);
            if (existingSpecialty != null)
            {
                throw new ArgumentException("Specialty already exists.");
            }

            await _unitOfWork.SpecialtyRepository.AddAsync(specialty);
            var specialtyDTO = _mapper.Map<SpecialtyModel>(specialty);
            return specialtyDTO;
        }

        public async Task<bool> DeleteSpecialtyAsync(string id)
        {
            var specialty = _unitOfWork.SpecialtyRepository.GetByIdAsync(id);
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            } else if (specialty == null)
            {
                throw new ArgumentException("Specialty not found.");
            }

            await _unitOfWork.SpecialtyRepository.DeleteAsync(id);          
            return true;
        }

        public async Task<IEnumerable<Specialties>> GetAllSpecialtiesAsync()
        {
            return await _unitOfWork.SpecialtyRepository.GetAllAsync();
        }

        public async Task<Specialties> GetSpecialtyByIdAsync(string id)
        {
            return await _unitOfWork.SpecialtyRepository.GetByIdAsync(id);
        }

        public async Task<Specialties> UpdateSpecialtyAsync(string id)
        {
            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(id);
            if (specialty == null)
            {
                throw new ArgumentNullException(nameof(specialty));
            }

            await _unitOfWork.SpecialtyRepository.UpdateAsync(specialty);
            return specialty;
        }

        
    }
}
