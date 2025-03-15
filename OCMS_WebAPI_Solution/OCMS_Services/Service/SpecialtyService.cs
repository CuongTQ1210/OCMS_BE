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

        public async Task<SpecialtyModel> AddSpecialtyAsync(SpecialtyModel specialtyViewModel, string createdByUserId)
        {
            var specialty = _mapper.Map<Specialties>(specialtyViewModel);
            var existingSpecialty = (await _unitOfWork.SpecialtyRepository.GetAllAsync()).FirstOrDefault(s => s.SpecialtyName == specialty.SpecialtyName);
            if (existingSpecialty != null)
            {
                throw new ArgumentException("Specialty already exists.");
            }

            specialty.CreatedByUserId = createdByUserId;
            specialty.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.SpecialtyRepository.AddAsync(specialty);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<SpecialtyModel>(specialty);
        }

        public async Task<bool> DeleteSpecialtyAsync(string id)
        {
            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(id);
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            else if (specialty == null)
            {
                throw new ArgumentException("Specialty not found.");
            }

            await _unitOfWork.SpecialtyRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SpecialtyModel>> GetAllSpecialtiesAsync()
        {
            var specialties = await _unitOfWork.SpecialtyRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<SpecialtyModel>>(specialties);
        }

        public async Task<SpecialtyModel> GetSpecialtyByIdAsync(string id)
        {
            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(id);
            return _mapper.Map<SpecialtyModel>(specialty);
        }

        public async Task<SpecialtyModel> UpdateSpecialtyAsync(string id, SpecialtyModel specialtyViewModel, string updatedByUserId)
        {
            var existingSpecialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(id);
            if (existingSpecialty == null)
            {
                throw new ArgumentException("Specialty not found.");
            }

            _mapper.Map(specialtyViewModel, existingSpecialty);
            existingSpecialty.UpdatedByUserId = updatedByUserId;
            existingSpecialty.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SpecialtyRepository.UpdateAsync(existingSpecialty);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<SpecialtyModel>(existingSpecialty);
        }

        
    }
}
