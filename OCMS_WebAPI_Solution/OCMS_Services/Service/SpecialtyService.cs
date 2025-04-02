using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Add Specialty
        public async Task<SpecialtyModel> AddSpecialtyAsync(CreateSpecialtyDTO model, string createdByUserId)
        {
            // Check if specialty with same name already exists
            var existingSpecialty = (await _unitOfWork.SpecialtyRepository.GetAllAsync())
                .FirstOrDefault(s => s.SpecialtyName.Equals(model.SpecialtyName, StringComparison.OrdinalIgnoreCase));

            if (existingSpecialty != null)
            {
                throw new ArgumentException("Specialty with this name already exists.");
            }

            // Generate specialty ID
            string specialtyId = await GenerateSpecialtyId(model.SpecialtyName, model.ParentSpecialtyId);

            // Create new specialty entity
            var specialty = new Specialties
            {
                SpecialtyId = specialtyId,
                SpecialtyName = model.SpecialtyName,
                Description = model.Description,
                ParentSpecialtyId = model.ParentSpecialtyId,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedByUserId = createdByUserId,
                UpdatedAt = DateTime.UtcNow,
                Status = model.Status
            };

            await _unitOfWork.SpecialtyRepository.AddAsync(specialty);
            await _unitOfWork.SaveChangesAsync();

            return await GetSpecialtyByIdAsync(specialtyId);
        }
        #endregion

        #region Delete Specialty
        public async Task<bool> DeleteSpecialtyAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(id);
            if (specialty == null)
            {
                throw new ArgumentException("Specialty not found.");
            }

            // Check if there are any subspecialties
            var hasSubSpecialties = (await _unitOfWork.SpecialtyRepository.GetAllAsync())
                .Any(s => s.ParentSpecialtyId == id);

            if (hasSubSpecialties)
            {
                throw new InvalidOperationException("Cannot delete specialty with subspecialties. Please delete or reassign all subspecialties first.");
            }

            await _unitOfWork.SpecialtyRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Get all specialties
        public async Task<IEnumerable<SpecialtyModel>> GetAllSpecialtiesAsync()
        {
            var specialties = await _unitOfWork.SpecialtyRepository.GetAllAsync();
            var specialtyModels = _mapper.Map<IEnumerable<SpecialtyModel>>(specialties);

            // Populate additional information for each specialty
            foreach (var model in specialtyModels)
            {
                if (!string.IsNullOrEmpty(model.ParentSpecialtyId))
                {
                    var parentSpecialty = specialties.FirstOrDefault(s => s.SpecialtyId == model.ParentSpecialtyId);
                    if (parentSpecialty != null)
                    {
                        model.ParentSpecialtyName = parentSpecialty.SpecialtyName;
                    }
                }

                if (!string.IsNullOrEmpty(model.CreatedByUserId))
                {
                    var createdByUser = await _unitOfWork.UserRepository.GetByIdAsync(model.CreatedByUserId);
                    if (createdByUser != null)
                    {
                        model.CreatedByUserName = createdByUser.FullName;
                    }
                }

                if (!string.IsNullOrEmpty(model.UpdatedByUserId))
                {
                    var updatedByUser = await _unitOfWork.UserRepository.GetByIdAsync(model.UpdatedByUserId);
                    if (updatedByUser != null)
                    {
                        model.UpdatedByUserName = updatedByUser.FullName;
                    }
                }
            }

            return specialtyModels;
        }
        #endregion

        #region Get Specialty Tree
        public async Task<IEnumerable<SpecialtyTreeModel>> GetSpecialtyTreeAsync()
        {
            var allSpecialties = await _unitOfWork.SpecialtyRepository.GetAllAsync();
            var specialtyModels = _mapper.Map<IEnumerable<SpecialtyTreeModel>>(allSpecialties);

            // Build the tree structure
            var tree = specialtyModels.Where(s => s.ParentSpecialtyId == null).ToList();
            var remainingSpecialties = specialtyModels.Where(s => s.ParentSpecialtyId != null).ToList();

            BuildSpecialtyTree(tree, remainingSpecialties);

            return tree;
        }

        private void BuildSpecialtyTree(IEnumerable<SpecialtyTreeModel> parentNodes, List<SpecialtyTreeModel> allNodes)
        {
            foreach (var parent in parentNodes)
            {
                var children = allNodes.Where(s => s.ParentSpecialtyId == parent.SpecialtyId).ToList();
                if (children.Any())
                {
                    parent.Children = children;
                    allNodes.RemoveAll(n => children.Contains(n));
                    BuildSpecialtyTree(children, allNodes);
                }
            }
        }
        #endregion

        #region Get Specialty by Id
        public async Task<SpecialtyModel> GetSpecialtyByIdAsync(string id)
        {
            var specialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(id);
            if (specialty == null)
            {
                return null;
            }

            var specialtyModel = _mapper.Map<SpecialtyModel>(specialty);

            // Get sub-specialties
            var subSpecialties = (await _unitOfWork.SpecialtyRepository.GetAllAsync())
                .Where(s => s.ParentSpecialtyId == id)
                .ToList();

            specialtyModel.SubSpecialties = _mapper.Map<ICollection<SpecialtyModel>>(subSpecialties);

            // Get parent specialty name
            if (!string.IsNullOrEmpty(specialtyModel.ParentSpecialtyId))
            {
                var parentSpecialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(specialtyModel.ParentSpecialtyId);
                if (parentSpecialty != null)
                {
                    specialtyModel.ParentSpecialtyName = parentSpecialty.SpecialtyName;
                }
            }

            // Get user names
            if (!string.IsNullOrEmpty(specialtyModel.CreatedByUserId))
            {
                var createdByUser = await _unitOfWork.UserRepository.GetByIdAsync(specialtyModel.CreatedByUserId);
                if (createdByUser != null)
                {
                    specialtyModel.CreatedByUserName = createdByUser.FullName;
                }
            }

            if (!string.IsNullOrEmpty(specialtyModel.UpdatedByUserId))
            {
                var updatedByUser = await _unitOfWork.UserRepository.GetByIdAsync(specialtyModel.UpdatedByUserId);
                if (updatedByUser != null)
                {
                    specialtyModel.UpdatedByUserName = updatedByUser.FullName;
                }
            }

            return specialtyModel;
        }
        #endregion

        #region Update Specialty
        public async Task<SpecialtyModel> UpdateSpecialtyAsync(string id, UpdateSpecialtyDTO model, string updatedByUserId)
        {
            var existingSpecialty = await _unitOfWork.SpecialtyRepository.GetByIdAsync(id);
            if (existingSpecialty == null)
            {
                throw new ArgumentException("Specialty not found.");
            }

            // Check for name conflicts (if name is changing)
            if (!existingSpecialty.SpecialtyName.Equals(model.SpecialtyName, StringComparison.OrdinalIgnoreCase))
            {
                var nameExists = (await _unitOfWork.SpecialtyRepository.GetAllAsync())
                    .Any(s => s.SpecialtyName.Equals(model.SpecialtyName, StringComparison.OrdinalIgnoreCase) && s.SpecialtyId != id);

                if (nameExists)
                {
                    throw new ArgumentException("Another specialty with this name already exists.");
                }
            }

            // Check for circular reference in parent-child relationship
            if (model.ParentSpecialtyId == id)
            {
                throw new ArgumentException("A specialty cannot be its own parent.");
            }

            if (!string.IsNullOrEmpty(model.ParentSpecialtyId))
            {
                // Check if creates a circular reference in the hierarchy
                string currentParentId = model.ParentSpecialtyId;
                while (!string.IsNullOrEmpty(currentParentId))
                {
                    var parent = await _unitOfWork.SpecialtyRepository.GetByIdAsync(currentParentId);
                    if (parent == null)
                    {
                        break;
                    }

                    if (parent.SpecialtyId == id)
                    {
                        throw new ArgumentException("Cannot set parent that would create a circular reference in the hierarchy.");
                    }

                    currentParentId = parent.ParentSpecialtyId;
                }
            }

            // Update properties
            existingSpecialty.SpecialtyName = model.SpecialtyName;
            existingSpecialty.Description = model.Description;
            existingSpecialty.ParentSpecialtyId = model.ParentSpecialtyId;
            existingSpecialty.Status = model.Status;
            existingSpecialty.UpdatedByUserId = updatedByUserId;
            existingSpecialty.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SpecialtyRepository.UpdateAsync(existingSpecialty);
            await _unitOfWork.SaveChangesAsync();

            return await GetSpecialtyByIdAsync(id);
        }
        #endregion

        #region Helper Methods
        public async Task<string> GenerateSpecialtyId(string specialtyName, string parentSpecialtyId = null)
        {
            string specialtyId;

            if (string.IsNullOrEmpty(parentSpecialtyId))
            {
                var words = specialtyName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 1)
                {
                    specialtyId = string.Join("", words.Select(w => char.ToUpper(w[0])));
                }
                else
                {
                    specialtyId = specialtyName.Length > 3
                        ? specialtyName.Substring(0, 3).ToUpper()
                        : specialtyName.ToUpper();
                }
            }
            else
            {
                string typeCode;
                var words = specialtyName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length > 1)
                {
                    typeCode = string.Join("", words.Take(Math.Min(3, words.Length)).Select(w => char.ToUpper(w[0])));
                }
                else
                {
                    int lettersToTake = Math.Min(3, specialtyName.Length);
                    typeCode = specialtyName.Substring(0, lettersToTake).ToUpper();
                }

                int sequenceNumber = 1;
                var specialtiesWithSameParent = (await _unitOfWork.SpecialtyRepository.GetAllAsync())
                    .Where(s => s.ParentSpecialtyId == parentSpecialtyId)
                    .ToList();

                if (specialtiesWithSameParent.Any())
                {
                    sequenceNumber = specialtiesWithSameParent.Count + 1;
                }

                specialtyId = $"{parentSpecialtyId}-{typeCode}-{sequenceNumber:D3}";
            }

            var existingWithSameId = await _unitOfWork.SpecialtyRepository.ExistsAsync(s => s.SpecialtyId == specialtyId);
            int counter = 1;
            string originalId = specialtyId;

            while (existingWithSameId)
            {
                if (string.IsNullOrEmpty(parentSpecialtyId))
                {
                    specialtyId = $"{originalId}{counter}";
                }
                else
                {
                    counter = int.Parse(originalId.Split('-').Last()) + 1;
                    string baseId = originalId.Substring(0, originalId.LastIndexOf('-') + 1);
                    specialtyId = $"{baseId}{counter:D3}";
                }

                counter++;
                existingWithSameId = await _unitOfWork.SpecialtyRepository.ExistsAsync(s => s.SpecialtyId == specialtyId);
            }

            return specialtyId;
        }
        #endregion
    }
}