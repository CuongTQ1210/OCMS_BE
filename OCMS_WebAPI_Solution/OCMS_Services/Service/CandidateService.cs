using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using OCMS_BOs.Entities;
using OCMS_BOs.ResponseModel;
using OCMS_Repositories;
using OCMS_Services.IService;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
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

        #region Get All Candidates
        public async Task<IEnumerable<Candidate>> GetAllCandidates()
        {
            return await _unitOfWork.CandidateRepository.GetAllAsync();
        }
        #endregion

        #region Get Candidate By Id
        public async Task<Candidate> GetCandidateByIdAsync(string id)
        {
            return await _unitOfWork.CandidateRepository.GetByIdAsync(id);
        }
        #endregion       

        #region Import Candidates new
        public async Task<ImportResult> ImportCandidatesFromExcelAsync(Stream fileStream, string importedByUserId, IBlobService blobService)
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
                    var candidateSheet = package.Workbook.Worksheets["Candidate"];
                    var certificateSheet = package.Workbook.Worksheets["ExternalCertificate"];

                    if (candidateSheet == null || certificateSheet == null)
                    {
                        result.Errors.Add("File Excel phải chứa cả 2 sheet: 'Candidate' và 'ExternalCertificate'");
                        return result;
                    }

                    // Lấy danh sách specialties để mapping
                    var specialties = await _unitOfWork.SpecialtyRepository.GetAllAsync();
                    var specialtyDict = specialties.ToDictionary(s => s.SpecialtyName.ToLower(), s => s.SpecialtyId);

                    // Lấy CandidateId cuối cùng để sinh ID mới
                    var lastCandidate = await GetLastCandidateIdAsync();
                    int lastIdNumber = 0;
                    if (!string.IsNullOrEmpty(lastCandidate))
                    {
                        string numericPart = new string(lastCandidate.Where(char.IsDigit).ToArray());
                        int.TryParse(numericPart, out lastIdNumber);
                    }

                    // Dictionary để mapping PersonalID sang CandidateId
                    var personalIdToCandidateId = new Dictionary<string, string>();
                    var personalIds = new HashSet<string>(); // Kiểm tra PersonalID trùng lặp
                    var candidates = new List<Candidate>();
                    var externalCertificates = new List<ExternalCertificate>();

                    // **Xử lý sheet Candidate**
                    int candidateRowCount = candidateSheet.Dimension.Rows;
                    result.TotalRecords = candidateRowCount - 1;

                    for (int row = 2; row <= candidateRowCount; row++)
                    {
                        if (IsRowEmpty(candidateSheet, row)) continue;

                        // Đọc và parse DateOfBirth
                        DateTime dateOfBirth;
                        var dobCell = candidateSheet.Cells[row, 3];
                        if (dobCell.Value == null || !TryParseDate(dobCell.Value, out dateOfBirth))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row} (Candidate): Invalid Date of Birth");
                            continue;
                        }

                        // Đọc SpecialtyName và lấy SpecialtyId
                        string specialtyName = candidateSheet.Cells[row, 8].GetValue<string>();
                        if (string.IsNullOrEmpty(specialtyName) || !specialtyDict.TryGetValue(specialtyName.ToLower(), out string specialtyId))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row} (Candidate): Invalid Specialty '{specialtyName}'");
                            continue;
                        }

                        // Đọc PersonalID và kiểm tra trùng lặp
                        string personalId = candidateSheet.Cells[row, 7].GetValue<string>();
                        if (string.IsNullOrEmpty(personalId))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row} (Candidate): PersonalID is required");
                            continue;
                        }
                        if (!personalIds.Add(personalId))
                        {
                            result.FailedCount++;
                            result.Errors.Add($"Error at row {row} (Candidate): Duplicate PersonalID '{personalId}'");
                            continue;
                        }

                        // Sinh CandidateId mới
                        lastIdNumber++;
                        string candidateId = $"CAN{lastIdNumber:D5}";

                        var candidate = new Candidate
                        {
                            CandidateId = candidateId,
                            FullName = candidateSheet.Cells[row, 1].GetValue<string>(),
                            Gender = candidateSheet.Cells[row, 2].GetValue<string>(),
                            DateOfBirth = dateOfBirth,
                            Address = candidateSheet.Cells[row, 4].GetValue<string>(),
                            Email = candidateSheet.Cells[row, 5].GetValue<string>(),
                            PhoneNumber = candidateSheet.Cells[row, 6].GetValue<string>(),
                            PersonalID = personalId,
                            SpecialtyId = specialtyId,
                            Note = candidateSheet.Cells[row, 9].GetValue<string>(),
                            CandidateStatus = CandidateStatus.Pending,
                            ImportByUserID = importedByUserId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        // Validate candidate
                        if (ValidateCandidate(candidate, result, row))
                        {
                            candidates.Add(candidate);
                            personalIdToCandidateId[personalId] = candidateId; // Lưu mapping
                            result.SuccessCount++;
                        }
                        else
                        {
                            result.FailedCount++;
                        }
                    }

                    // **Xử lý sheet ExternalCertificate**
                    int certificateRowCount = certificateSheet.Dimension.Rows;
                    var drawings = certificateSheet.Drawings;

                    for (int row = 2; row <= certificateRowCount; row++)
                    {
                        if (IsRowEmpty(certificateSheet, row)) continue;

                        // Đọc PersonalID và tìm CandidateId tương ứng
                        string personalId = certificateSheet.Cells[row, 1].GetValue<string>();
                        if (string.IsNullOrEmpty(personalId) || !personalIdToCandidateId.TryGetValue(personalId, out string candidateId))
                        {
                            result.Errors.Add($"Error at row {row} (ExternalCertificate): Invalid PersonalID '{personalId}'");
                            continue;
                        }

                        // Đọc thông tin certificate
                        string certificateCode = certificateSheet.Cells[row, 2].GetValue<string>();
                        string certificateName = certificateSheet.Cells[row, 3].GetValue<string>();
                        string issuingOrganization = certificateSheet.Cells[row, 4].GetValue<string>();

                        // Trích xuất và upload ảnh bằng IBlobService
                        string certificateFileURL = null;
                        var drawing = drawings.FirstOrDefault(d => d.From.Row + 1 == row && d.From.Column + 1 == 5); // Cột 5 là ảnh
                        if (drawing != null && drawing is ExcelPicture picture)
                        {
                            string blobName = $"{candidateId}_{certificateCode}_{DateTime.UtcNow.Ticks}.jpg";
                            using (var stream = new MemoryStream(picture.Image.ImageBytes))
                            {
                                certificateFileURL = await blobService.UploadFileAsync("externalcertificate", blobName, stream);
                            }
                        }
                        else
                        {
                            result.Errors.Add($"Error at row {row} (ExternalCertificate): No image found for certificate");
                            continue;
                        }

                        var certificate = new ExternalCertificate
                        {
                            CandidateId = candidateId,
                            CertificateCode = certificateCode,
                            CertificateName = certificateName,
                            IssuingOrganization = issuingOrganization,
                            CertificateFileURL = certificateFileURL,
                            VerifyByUserId = importedByUserId,
                            VerifyDate = DateTime.UtcNow,
                            VerificationStatus = VerificationStatus.Pending,
                            CreatedAt = DateTime.UtcNow
                        };

                        externalCertificates.Add(certificate);
                    }

                    // **Lưu dữ liệu vào database**
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        await _unitOfWork.CandidateRepository.AddRangeAsync(candidates);
                        await _unitOfWork.ExternalCertificateRepository.AddRangeAsync(externalCertificates);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        result.Errors.Add($"Error saving data: {ex.Message}");
                        throw;
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
        private bool IsRowEmpty(ExcelWorksheet worksheet, int row)
        {
            int totalColumns = worksheet.Dimension.Columns;
            for (int col = 1; col <= totalColumns; col++)
            {
                if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].GetValue<string>()))
                {
                    return false;
                }
            }
            return true;
        }

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

        private bool TryParseDate(object value, out DateTime date)
        {
            date = DateTime.MinValue;
            if (value is DateTime dateValue)
            {
                date = dateValue;
                return true;
            }
            if (value is double numericDate)
            {
                date = DateTime.FromOADate(numericDate);
                return true;
            }
            return DateTime.TryParse(value?.ToString(), out date);
        }
        #endregion
    }
}