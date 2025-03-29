    using AutoMapper;
    using Azure.Storage.Blobs;
    using Microsoft.Extensions.Configuration;
    using OCMS_BOs.Entities;
    using OCMS_BOs.RequestModel;
    using OCMS_BOs.ResponseModel;
    using OCMS_Repositories;
    using OCMS_Repositories.IRepository;
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
            private readonly INotificationService _notificationService;
            private readonly IUserRepository _userRepository;
            private readonly ICandidateRepository _candidateRepository;
            private readonly IExternalCertificateService _externalCertificateService;

            public CandidateService(UnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IUserRepository userRepository, ICandidateRepository candidateRepository, IExternalCertificateService externalCertificateService)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _notificationService = notificationService;
                _userRepository = userRepository;
                _candidateRepository = candidateRepository;
                _externalCertificateService = externalCertificateService;
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

            #region Import Candidates
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
                    // Lấy tất cả candidates từ DB để kiểm tra trùng lặp
                    var existingCandidates = await _unitOfWork.CandidateRepository.GetAllAsync();
                    var existingPersonalIds = existingCandidates.Select(c => c.PersonalID).ToList();
                    var existingEmails = existingCandidates.Select(c => c.Email).ToList();
                    var existingPhoneNumbers = existingCandidates.Select(c => c.PhoneNumber).ToList();

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
                        var personalIds = new HashSet<string>(); // Kiểm tra PersonalID trùng lặp trong file
                        var emails = new HashSet<string>(); // Kiểm tra Email trùng lặp trong file
                        var phoneNumbers = new HashSet<string>(); // Kiểm tra PhoneNumber trùng lặp trong file
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

                            // Kiểm tra PersonalID trùng lặp trong file
                            if (!personalIds.Add(personalId))
                            {
                                result.FailedCount++;
                                result.Errors.Add($"Error at row {row} (Candidate): Duplicate PersonalID '{personalId}' within the import file");
                                continue;
                            }

                            // Kiểm tra PersonalID trùng lặp trong DB
                            if (existingPersonalIds.Contains(personalId))
                            {
                                result.FailedCount++;
                                result.Errors.Add($"Error at row {row} (Candidate): PersonalID '{personalId}' already exists in the database");
                                continue;
                            }

                            // Đọc Email và kiểm tra trùng lặp
                            string email = candidateSheet.Cells[row, 5].GetValue<string>();
                            if (!string.IsNullOrEmpty(email))
                            {
                                // Kiểm tra Email trùng lặp trong file
                                if (!emails.Add(email))
                                {
                                    result.FailedCount++;
                                    result.Errors.Add($"Error at row {row} (Candidate): Duplicate Email '{email}' within the import file");
                                    continue;
                                }

                                // Kiểm tra Email trùng lặp trong DB
                                if (existingEmails.Contains(email))
                                {
                                    result.FailedCount++;
                                    result.Errors.Add($"Error at row {row} (Candidate): Email '{email}' already exists in the database");
                                    continue;
                                }
                            }

                            // Đọc PhoneNumber và kiểm tra trùng lặp
                            string phoneNumber = candidateSheet.Cells[row, 6].GetValue<string>();
                            if (!string.IsNullOrEmpty(phoneNumber))
                            {
                                // Kiểm tra PhoneNumber trùng lặp trong file
                                if (!phoneNumbers.Add(phoneNumber))
                                {
                                    result.FailedCount++;
                                    result.Errors.Add($"Error at row {row} (Candidate): Duplicate Phone Number '{phoneNumber}' within the import file");
                                    continue;
                                }

                                // Kiểm tra PhoneNumber trùng lặp trong DB
                                if (existingPhoneNumbers.Contains(phoneNumber))
                                {
                                    result.FailedCount++;
                                    result.Errors.Add($"Error at row {row} (Candidate): Phone Number '{phoneNumber}' already exists in the database");
                                    continue;
                                }
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
                                Email = email,
                                PhoneNumber = phoneNumber,
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
                                    certificateFileURL = await blobService.UploadFileAsync("externalcertificates", blobName, stream);
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

                        // Nếu có lỗi, dừng việc import
                        if (result.FailedCount > 0)
                        {
                            result.SuccessCount = 0;
                            return result;
                        }

                        // **Lưu dữ liệu vào database**
                        await _unitOfWork.ExecuteWithStrategyAsync(async () =>
                        {
                            await _unitOfWork.BeginTransactionAsync();
                            try
                            {

                                if (result.SuccessCount > 0)
                                {
                                    var requestService = new RequestService(_unitOfWork, _mapper, _notificationService, _userRepository,_candidateRepository);
                                    var requestDto = new RequestDTO
                                    {
                                        RequestType = RequestType.CandidateImport,
                                        Description = $"Yêu cầu xác nhận danh sách {result.SuccessCount} ứng viên vừa được import",
                                        Notes = "Danh sách ứng viên cần phê duyệt"
                                    };


                                    var importRequestId = await requestService.CreateRequestAsync(requestDto, importedByUserId);
                                    foreach (var candidate in candidates)
                                    {
                                        candidate.ImportRequestId = importRequestId.RequestId;
                                    }

                                }
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
                        });

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

            #region Update Candidate
            public async Task<CandidateUpdateResponse> UpdateCandidateAsync(string id, CandidateUpdateDTO updatedCandidateModel)
            {
                var response = new CandidateUpdateResponse();

                try
                {
                    // Lấy ứng viên hiện tại từ database
                    var existingCandidate = await _unitOfWork.CandidateRepository.GetByIdAsync(id);
                    if (existingCandidate == null)
                    {
                        response.Success = false;
                        response.Message = $"Không tìm thấy ứng viên với ID {id}";
                        return response;
                    }

                    // Kiểm tra trùng lặp email nếu có thay đổi
                    if (updatedCandidateModel.Email != existingCandidate.Email)
                    {
                        var duplicateEmails = await _unitOfWork.CandidateRepository.FindAsync(c => c.Email == updatedCandidateModel.Email && c.CandidateId != id);
                        if (duplicateEmails.Any())
                        {
                            response.Success = false;
                            response.Message = $"Email {updatedCandidateModel.Email} đã được sử dụng bởi ứng viên khác";
                            return response;
                        }
                    }

                    if (updatedCandidateModel.PersonalID != existingCandidate.PersonalID)
                    {
                        var duplicatePersonalIDs = await _unitOfWork.CandidateRepository.FindAsync(c => c.PersonalID == updatedCandidateModel.PersonalID && c.CandidateId != id);
                        if (duplicatePersonalIDs.Any())
                        {
                            response.Success = false;
                            response.Message = $"Personal ID {updatedCandidateModel.PersonalID} đã được sử dụng bởi ứng viên khác";
                            return response;
                        }
                    }

                    if (updatedCandidateModel.PhoneNumber != existingCandidate.PhoneNumber)
                    {
                        var duplicatePhoneNumbers = await _unitOfWork.CandidateRepository.FindAsync(c => c.PhoneNumber == updatedCandidateModel.PhoneNumber && c.CandidateId != id);
                        if (duplicatePhoneNumbers.Any())
                        {
                            response.Success = false;
                            response.Message = $"Số điện thoại {updatedCandidateModel.PhoneNumber} đã được sử dụng bởi ứng viên khác";
                            return response;
                        }
                    }

                    // Ánh xạ dữ liệu từ DTO sang entity
                    _mapper.Map(updatedCandidateModel, existingCandidate);

                    // Cập nhật trường UpdatedAt với thời gian hiện tại
                    existingCandidate.UpdatedAt = DateTime.UtcNow;

                    // Lưu thay đổi vào database
                    await _unitOfWork.CandidateRepository.UpdateAsync(existingCandidate);
                    await _unitOfWork.SaveChangesAsync();

                    // Thiết lập response thành công
                    response.Candidate = existingCandidate;
                    response.Message = "Cập nhật ứng viên thành công";
                    response.Success = true;
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có
                    response.Success = false;
                    response.Message = $"Đã xảy ra lỗi: {ex.Message}";
                }

                return response;
            }
            #endregion

            #region Delete Candidate
            public async Task<(bool success, string message)> DeleteCandidateAsync(string id)
            {
                var candidate = await _unitOfWork.CandidateRepository.GetByIdAsync(id);
                if (candidate == null)
                {
                    return (false, "Candidate not found");
                }

                // Check if candidate has any certificates
                var certificates = await _unitOfWork.ExternalCertificateRepository.FindAsync(c => c.CandidateId == id);
                if (certificates.Any())
                {
                    return (false, "Please clear all external certificates from this candidate first");
                }

                // Execute with transaction to ensure database consistency
                bool success = false;
                await _unitOfWork.ExecuteWithStrategyAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Delete the candidate
                        await _unitOfWork.CandidateRepository.DeleteAsync(id);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        success = true;
                    }
                    catch (Exception)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw;
                    }
                });

                return (success, success ? "Candidate deleted successfully" : "Failed to delete candidate");
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