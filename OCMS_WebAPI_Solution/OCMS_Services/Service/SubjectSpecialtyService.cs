using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories.IRepository;
using OCMS_Services.IService;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using OCMS_Repositories;

namespace OCMS_Services.Service
{
    public class SubjectSpecialtyService : ISubjectSpecialtyService
    {
        private readonly ISubjectSpecialtyRepository _repository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;

        public SubjectSpecialtyService(ISubjectSpecialtyRepository repository, ISubjectRepository subjectRepository, ISpecialtyRepository specialtyRepository, IMapper mapper, UnitOfWork unitOfWork)
        {
            _repository = repository;
            _subjectRepository = subjectRepository;
            _specialtyRepository = specialtyRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SubjectSpecialtyModel>> GetAllAsync()
        {
            var entities = await _repository.GetAllWithIncludesAsync();
            return _mapper.Map<IEnumerable<SubjectSpecialtyModel>>(entities);
        }

        public async Task<SubjectSpecialtyModel> GetByIdAsync(string id)
        {
            var entity = await _repository.GetWithIncludesAsync(id);
            return _mapper.Map<SubjectSpecialtyModel>(entity);
        }

        private async Task<string> GenerateSubjectSpecialtyId(string subjectName, string specialtyName)
        {
            // Get initials from subject name (up to 2 words)
            string subjectInitials = string.Concat(subjectName
                .Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Take(2)
                .Select(word => char.ToUpper(word[0])));

            if (string.IsNullOrEmpty(subjectInitials))
            {
                subjectInitials = "SB"; // Default if no valid initials
            }

            // Get initials from specialty name (up to 2 words)
            string specialtyInitials = string.Concat(specialtyName
                .Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Take(2)
                .Select(word => char.ToUpper(word[0])));

            if (string.IsNullOrEmpty(specialtyInitials))
            {
                specialtyInitials = "SP"; // Default if no valid initials
            }

            // Combine initials
            string combinedInitials = $"{subjectInitials}{specialtyInitials}";

            // Get the last SubjectSpecialty ID with these initials
            var allSubjectSpecialties = await _unitOfWork.SubjectSpecialtyRepository.GetAllAsync();
            var lastId = allSubjectSpecialties
                .Where(ss => ss.SubjectSpecialtyId.StartsWith(combinedInitials))
                .OrderByDescending(ss => ss.SubjectSpecialtyId)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastId != null)
            {
                // Extract the numeric part and increment
                string numericPart = new string(lastId.SubjectSpecialtyId.SkipWhile(c => !char.IsDigit(c)).ToArray());
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // Format the new ID with leading zeros
            return $"{combinedInitials}{nextNumber:D3}";
        }

        public async Task<SubjectSpecialtyModel> AddAsync(SubjectSpecialtyDTO dto)
        {
            // Get subject and specialty names
            var subject = await _unitOfWork.SubjectRepository.GetByIdAsync(dto.SubjectId);
            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(dto.SpecialtyId);
            if (subject == null || specialty == null)
                throw new System.Exception("Subject or Specialty not found");

            // Check for existing SubjectSpecialty
            var existing = await _unitOfWork.SubjectSpecialtyRepository.FirstOrDefaultAsync(
                ss => ss.SubjectId == dto.SubjectId && ss.SpecialtyId == dto.SpecialtyId);
            if (existing != null)
                throw new System.Exception("A SubjectSpecialty with the same Subject and Specialty already exists.");

            string generatedId = await GenerateSubjectSpecialtyId(subject.SubjectName, specialty.SpecialtyName);

            var entity = _mapper.Map<SubjectSpecialty>(dto);
            entity.SubjectSpecialtyId = generatedId;

            await _unitOfWork.SubjectSpecialtyRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<SubjectSpecialtyModel>(entity);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            await _unitOfWork.SubjectSpecialtyRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
} 