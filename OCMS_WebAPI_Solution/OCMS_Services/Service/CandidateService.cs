using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.ResponseModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS_Services.Service
{
    public class CandidateService : ICandidateService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CandidateService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Candidate>> GetAllCandidates()
        {
            return await _unitOfWork.CandidateRepository.GetAllAsync();
        }

        #region Get Candidate By Id
        public async Task<Candidate> GetCandidateByIdAsync(string id)
        {
            return await _unitOfWork.CandidateRepository.GetByIdAsync(id);
        }
        #endregion

        #region Import Candidates
        public async Task<ImportResult> ImportCandidatesFromExcelAsync(Stream fileStream, string importedByUserId)
        {
            var result = new ImportResult
            {
                TotalRecords = 0,
                SuccessCount = 0,
                FailedCount = 0,
                Errors = new List<string>()
            };

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // First worksheet
                    int rowCount = worksheet.Dimension.Rows;

                    result.TotalRecords = rowCount - 1; // Exclude header row
                    var candidates = new List<Candidate>();

                    // Get all specialties for validation and mapping
                    var specialties = await _unitOfWork.SpecialtyRepository.GetAllAsync();
                    var specialtyDict = specialties.ToDictionary(s => s.SpecialtyName.ToLower(), s => s);

                    // Get existing candidates for ID generation
                    var lastCandidate = await GetLastCandidateIdAsync();
                    int lastIdNumber = 0;

                    if (!string.IsNullOrEmpty(lastCandidate))
                    {
                        // Extract the numeric part of the candidate ID
                        // Assuming format is like "C0001", "C0002", etc.
                        string numericPart = new string(lastCandidate.Where(char.IsDigit).ToArray());
                        int.TryParse(numericPart, out lastIdNumber);
                    }

                    // Start from row 2 (after header)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            // Handle empty rows
                            if (IsRowEmpty(worksheet, row))
                            {
                                continue;
                            }

                            // Handle date properly from Excel
                            DateTime dateOfBirth;
                            var dobCell = worksheet.Cells[row, 3];

                            // Try different methods to parse the date
                            if (dobCell.Value == null)
                            {
                                result.FailedCount++;
                                result.Errors.Add($"Error at row {row}: Date of Birth is required");
                                continue;
                            }
                            else if (dobCell.Value is DateTime dateValue)
                            {
                                dateOfBirth = dateValue;
                            }
                            else if (dobCell.Value is double numericDate)
                            {
                                // Excel stores dates as serial numbers
                                dateOfBirth = DateTime.FromOADate(numericDate);
                            }
                            else
                            {
                                // Try to parse as string if not a numeric date
                                if (!DateTime.TryParse(dobCell.Value.ToString(), out dateOfBirth))
                                {
                                    result.FailedCount++;
                                    result.Errors.Add($"Error at row {row}: Invalid date format: {dobCell.Value}");
                                    continue;
                                }
                            }

                            // Get specialty ID by name
                            string specialtyName = worksheet.Cells[row, 9].GetValue<string>();

                            // Validate specialty exists
                            if (string.IsNullOrEmpty(specialtyName))
                            {
                                result.FailedCount++;
                                result.Errors.Add($"Error at row {row}: Specialty name is required");
                                continue;
                            }

                            string specialtyId = null;
                            if (specialtyDict.TryGetValue(specialtyName.ToLower(), out var specialty))
                            {
                                specialtyId = specialty.SpecialtyId;
                            }
                            else
                            {
                                result.FailedCount++;
                                result.Errors.Add($"Error at row {row}: Specialty '{specialtyName}' not found");
                                continue;
                            }

                            // Generate new candidate ID
                            lastIdNumber++;
                            string candidateId = $"CAN{lastIdNumber:D5}"; // Format: CAN00001, CAN00002, etc.

                            var candidate = new Candidate
                            {
                                CandidateId = candidateId,
                                FullName = worksheet.Cells[row, 1].GetValue<string>(),
                                Gender = worksheet.Cells[row, 2].GetValue<string>(),
                                DateOfBirth = dateOfBirth,
                                Address = worksheet.Cells[row, 4].GetValue<string>(),
                                Email = worksheet.Cells[row, 5].GetValue<string>(),
                                PhoneNumber = worksheet.Cells[row, 6].GetValue<string>(),
                                PersonalID = worksheet.Cells[row, 7].GetValue<string>(),
                                ExternalCertificate = worksheet.Cells[row, 8].GetValue<string>(),
                                SpecialtyId = specialtyId,
                                Note = worksheet.Cells[row, 10].GetValue<string>(),
                                CandidateStatus = CandidateStatus.Pending,
                                ImportByUserID = importedByUserId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            // Validate candidate data
                            if (ValidateCandidate(candidate, result, row))
                            {
                                candidates.Add(candidate);
                                result.SuccessCount++;
                            }
                            else
                            {
                                result.FailedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row}: {ex.Message}");
                        }
                    }

                    // Save all valid candidates
                    if (candidates.Any())
                    {
                        await _unitOfWork.CandidateRepository.AddRangeAsync(candidates);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"General error: {ex.Message}");
                result.FailedCount = result.TotalRecords;
                result.SuccessCount = 0;
            }

            return result;
        }
        #endregion

        #region Helper Methods
        private async Task<string> GetLastCandidateIdAsync()
        {
            var candidates = await _unitOfWork.CandidateRepository.GetAllAsync();

            if (!candidates.Any())
                return null;

            return candidates
                .Select(c => c.CandidateId)
                .OrderByDescending(id =>
                {
                    // Extract numeric part for sorting
                    string numericPart = new string(id.Where(char.IsDigit).ToArray());
                    if (int.TryParse(numericPart, out int number))
                        return number;
                    return 0;
                })
                .FirstOrDefault();
        }

        private bool IsRowEmpty(ExcelWorksheet worksheet, int row)
        {
            for (int col = 1; col <= 10; col++)
            {
                if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].GetValue<string>()))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidateCandidate(Candidate candidate, ImportResult result, int row)
        {
            bool isValid = true;

            // Validate required fields
            if (string.IsNullOrEmpty(candidate.FullName))
            {
                result.Errors.Add($"Error at row {row}: Full name is required");
                isValid = false;
            }

            if (string.IsNullOrEmpty(candidate.Gender))
            {
                result.Errors.Add($"Error at row {row}: Gender is required");
                isValid = false;
            }

            if (string.IsNullOrEmpty(candidate.Email))
            {
                result.Errors.Add($"Error at row {row}: Email is required");
                isValid = false;
            }
            else if (!IsValidEmail(candidate.Email))
            {
                result.Errors.Add($"Error at row {row}: Invalid email format");
                isValid = false;
            }

            if (string.IsNullOrEmpty(candidate.PhoneNumber))
            {
                result.Errors.Add($"Error at row {row}: Phone number is required");
                isValid = false;
            }

            if (string.IsNullOrEmpty(candidate.PersonalID))
            {
                result.Errors.Add($"Error at row {row}: Personal ID is required");
                isValid = false;
            }

            return isValid;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}