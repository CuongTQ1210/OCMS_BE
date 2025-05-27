using AutoMapper;
using OCMS_BOs.Entities;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ViewModel;
using OCMS_Repositories.IRepository;
using OCMS_Repositories;
using OCMS_Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using OfficeOpenXml;
using OCMS_BOs.ResponseModel;

namespace OCMS_Services.Service
{
    public class CourseService : ICourseService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly IRequestService _requestService;

        public CourseService(UnitOfWork unitOfWork, IMapper mapper, ICourseRepository courseRepository, IRequestService requestService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
        }

        #region Create Course
        public async Task<CourseModel> CreateCourseAsync(CourseDTO dto, string createdByUserId)
        {
            if (string.IsNullOrEmpty(createdByUserId))
                throw new ArgumentException("Creator user ID cannot be empty.");

            // Convert empty CourseRelatedId to null
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
            {
                dto.CourseRelatedId = null;
            }

            // Parse CourseLevel from DTO
            if (!Enum.TryParse<CourseLevel>(dto.CourseLevel, true, out var courseLevel))
                throw new ArgumentException("Invalid CourseLevel provided.");

            // Validate CourseRelatedId based on CourseLevel
            if (courseLevel == CourseLevel.Initial)
            {
                if (!string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new ArgumentException("CourseRelatedId must be null for Initial course level.");
            }
            else if (courseLevel == CourseLevel.Professional)
            {
                if (string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new ArgumentException("CourseRelatedId is required for Professional course level.");

                var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
                if (relatedCourse == null)
                    throw new ArgumentException("The specified CourseRelatedId does not exist.");
                if (relatedCourse.CourseLevel != CourseLevel.Initial)
                    throw new ArgumentException("The related course for a Professional course must have Initial level.");
            }
            else if (courseLevel == CourseLevel.Recurrent)
            {
                if (string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new ArgumentException("CourseRelatedId is required for Recurrent course level.");

                var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
                if (relatedCourse == null)
                    throw new ArgumentException("The specified CourseRelatedId does not exist.");
                if (relatedCourse.CourseLevel != CourseLevel.Initial && relatedCourse.CourseLevel != CourseLevel.Professional)
                    throw new ArgumentException("The related course for a Recurrent course must have Initial or Professional level.");
            }
            else if (courseLevel == CourseLevel.Relearn)
            {
                if (string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new ArgumentException("CourseRelatedId is required for Relearn course level.");

                var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
                if (relatedCourse == null)
                    throw new ArgumentException("The specified CourseRelatedId does not exist.");
                if (relatedCourse.CourseLevel != CourseLevel.Initial &&
                    relatedCourse.CourseLevel != CourseLevel.Professional &&
                    relatedCourse.CourseLevel != CourseLevel.Recurrent)
                    throw new ArgumentException("The related course for a Relearn course must have Initial, Professional, or Recurrent level.");
            }
            else
            {
                throw new ArgumentException("Unsupported CourseLevel provided.");
            }

            var courseId = await GenerateCourseId(dto.CourseName, dto.CourseLevel, dto.CourseRelatedId);
            var existedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            if (existedCourse != null)
            {
                throw new ArgumentException($"Course with this level {dto.CourseLevel} already existed for this course {dto.CourseRelatedId}");
            }

            // Map DTO to Course entity
            var course = _mapper.Map<Course>(dto);
            course.CourseId = courseId;
            course.CreatedByUserId = createdByUserId;
            course.CreatedAt = DateTime.Now;
            course.UpdatedAt = DateTime.Now;
            course.Status = CourseStatus.Pending;
            course.Progress = Progress.NotYet;
            course.StartDateTime= DateTime.Now;
            course.EndDateTime = DateTime.Now.AddDays(365);
            course.CourseLevel = courseLevel;

            // Add course to repository and save
            await _unitOfWork.CourseRepository.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            // Create approval request
            var requestDto = new RequestDTO
            {
                RequestEntityId = courseId,
                RequestType = RequestType.NewCourse,
                Description = $"Request to approve new course '{course.CourseName}'",
                Notes = $"Course details:\n" +
                       $"Level: {course.CourseLevel}\n" +
                       $"Start Date: {course.StartDateTime}\n" +
                       $"End Date: {course.EndDateTime}\n"
            };
            await _requestService.CreateRequestAsync(requestDto, createdByUserId);

            return _mapper.Map<CourseModel>(course);
        }
        #endregion

        #region Get All Courses
        public async Task<IEnumerable<CourseModel>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetAllWithIncludesAsync(query =>
                query.Include(c => c.SubjectSpecialties)
                     .ThenInclude(ss => ss.Subject)
                     .Include(c => c.SubjectSpecialties)
                     .ThenInclude(ss => ss.Specialty)
                     .Include(c => c.CreatedByUser)
                     .Include(c => c.RelatedCourse)
            );

            return _mapper.Map<IEnumerable<CourseModel>>(courses);
        }
        #endregion

        #region Get Course by ID
        public async Task<CourseModel?> GetCourseByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Course ID cannot be empty.");

            var course = await _courseRepository.GetWithIncludesAsync(
                c => c.CourseId == id,
                query => query.Include(c => c.SubjectSpecialties)
                     .ThenInclude(ss => ss.Subject)
                     .Include(c => c.SubjectSpecialties)
                     .ThenInclude(ss => ss.Specialty)
                     .Include(c => c.CreatedByUser)
                     .Include(c => c.RelatedCourse)
            );

            return course == null ? null : _mapper.Map<CourseModel>(course);
        }
        #endregion

        #region Get Course by Class Id
        public async Task<CourseModel>GetCourseByClassIdAsync(string classId)
        {
            if (string.IsNullOrEmpty(classId))
                throw new ArgumentException("Class ID cannot be empty.");
            var course = await _courseRepository.GetCourseByClassIdAsync(classId);
            return _mapper.Map<CourseModel>(course);
        }
        #endregion

        #region Delete Course
        public async Task<bool> DeleteCourseAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Course ID cannot be empty.");

            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
            if (course == null)
                throw new Exception("Course does not exist.");

            // Check if there are any certificates linked to this course
            var linkedCertificates = await _unitOfWork.CertificateRepository.GetAllAsync(c => c.CourseId == id);
            if (linkedCertificates.Any())
            {
                throw new InvalidOperationException($"Cannot delete course {id} because it has linked certificates.");
            }

            if (course.Status == CourseStatus.Approved)
            {
                // Create a request for deletion
                var requestDto = new RequestDTO
                {
                    RequestType = RequestType.DeleteCourse,
                    RequestEntityId = id,
                    Description = $"Request to delete course {course.CourseName} ({id})",
                    Notes = "Awaiting HeadMaster approval"
                };
                await _requestService.CreateRequestAsync(requestDto, course.CreatedByUserId);
                throw new InvalidOperationException($"Cannot delete course {id} because it is Approved. A request has been sent to the HeadMaster for approval.");
            }

            // Check if the course has any related courses
            var relatedCourses = await _unitOfWork.CourseRepository.GetAllAsync(c => c.RelatedCourseId == id);
            if (relatedCourses.Any())
            {
                throw new InvalidOperationException($"Cannot delete course {id} because it has related courses: {string.Join(", ", relatedCourses.Select(c => c.CourseId))}");
            }

            // Allow deletion only for Pending or Rejected statuses
            if (course.Status == CourseStatus.Pending || course.Status == CourseStatus.Rejected)
            {
                await _unitOfWork.CourseRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            throw new InvalidOperationException($"Cannot delete course {id} because its status is {course.Status}. Only Pending or Rejected courses can be deleted.");
        }
        #endregion

        #region Update Course
        public async Task<CourseModel> UpdateCourseAsync(string id, CourseUpdateDTO dto, string updatedByUserId)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Course ID cannot be empty.");

            if (string.IsNullOrEmpty(updatedByUserId))
                throw new ArgumentException("Updater user ID cannot be empty.");

            // Validate course existence
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);
            if (course == null)
                throw new Exception("Course Id does not exist!");

            // Check if the course is approved and create a change request instead of direct update
            if (course.Status == CourseStatus.Approved)
            {
                var requestDto = new RequestDTO
                {
                    RequestType = RequestType.UpdateCourse,
                    RequestEntityId = id,
                    Description = $"Request to update course '{course.CourseName}'",
                    Notes = $"Requested changes: {JsonSerializer.Serialize(dto)}"
                };
                await _requestService.CreateRequestAsync(requestDto, updatedByUserId);
                throw new InvalidOperationException($"Course {id} is already approved. A change request has been created and is awaiting approval.");
            }

            // Convert empty CourseRelatedId to null
            if (string.IsNullOrEmpty(dto.CourseRelatedId))
            {
                dto.CourseRelatedId = null;
            }

            // Validate CourseRelatedId based on CourseLevel
            if (course.CourseLevel == CourseLevel.Initial)
            {
                if (!string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new ArgumentException("CourseRelatedId must be null for Initial course level.");
            }
            else if (course.CourseLevel == CourseLevel.Professional)
            {
                if (string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new ArgumentException("CourseRelatedId is required for Professional course level.");

                var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
                if (relatedCourse == null)
                    throw new ArgumentException("The specified CourseRelatedId does not exist.");
                if (relatedCourse.CourseLevel != CourseLevel.Initial)
                    throw new ArgumentException("The related course for a Professional course must have Initial level.");
            }
            else if (course.CourseLevel == CourseLevel.Recurrent)
            {
                if (string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new ArgumentException("CourseRelatedId is required for Recurrent course level.");

                var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
                if (relatedCourse == null)
                    throw new ArgumentException("The specified CourseRelatedId does not exist.");
                if (relatedCourse.CourseLevel != CourseLevel.Initial && relatedCourse.CourseLevel != CourseLevel.Professional)
                    throw new ArgumentException("The related course for a Recurrent course must have Initial or Professional level.");
            }
            else if (course.CourseLevel == CourseLevel.Relearn)
            {
                if (string.IsNullOrEmpty(dto.CourseRelatedId))
                    throw new ArgumentException("CourseRelatedId is required for Relearn course level.");

                var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(dto.CourseRelatedId);
                if (relatedCourse == null)
                    throw new ArgumentException("The specified CourseRelatedId does not exist.");
                if (relatedCourse.CourseLevel != CourseLevel.Initial &&
                    relatedCourse.CourseLevel != CourseLevel.Professional &&
                    relatedCourse.CourseLevel != CourseLevel.Recurrent)
                    throw new ArgumentException("The related course for a Relearn course must have Initial, Professional, or Recurrent level.");
            }
            else
            {
                throw new ArgumentException("Unsupported CourseLevel in course.");
            }            

            // Parse and validate dates
            if (!dto.StartDate.HasValue || !dto.EndDate.HasValue)
                throw new ArgumentException("StartDate and EndDate are required.");

            if (dto.StartDate < DateTime.Today)
                throw new ArgumentException("StartDate cannot be in the past.");

            if (dto.StartDate.Value >= dto.EndDate.Value)
                throw new ArgumentException("StartDate must be earlier than EndDate.");

            // Map DTO to course entity
            _mapper.Map(dto, course);
            course.UpdatedAt = DateTime.Now;

            // Update course in repository and save
            await _unitOfWork.CourseRepository.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseModel>(course);
        }
        #endregion

        #region Send Course Request For Approval
        public async Task<bool> SendCourseRequestForApprove(string courseId, string createdByUserId)
        {
            if (string.IsNullOrEmpty(courseId))
                throw new ArgumentException("Course ID cannot be empty.");

            if (string.IsNullOrEmpty(createdByUserId))
                throw new ArgumentException("User ID cannot be empty.");

            var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException($"Course with id {courseId} does not exist!");
            }

            // Check if the course is already approved
            if (course.Status == CourseStatus.Approved)
            {
                throw new InvalidOperationException($"Course {courseId} is already approved.");
            }

            // Check if there's already a pending request for this course
            var pendingRequests = await _unitOfWork.RequestRepository.GetAllAsync(
                r => r.RequestEntityId == courseId &&
                     (r.RequestType == RequestType.NewCourse || r.RequestType == RequestType.UpdateCourse) &&
                     r.Status == RequestStatus.Pending
            );

            if (pendingRequests.Any())
            {
                throw new InvalidOperationException($"There is already a pending approval request for course {courseId}.");
            }

            var requestDto = new RequestDTO
            {
                RequestEntityId = courseId,
                RequestType = RequestType.NewCourse,
                Description = $"Request to approve new course '{course.CourseName}'",
                Notes = null
            };

            await _requestService.CreateRequestAsync(requestDto, createdByUserId);
            return true;
        }
        #endregion

        #region Helper Methods
        private async Task<string> GenerateSubjectSpecialtyId(string subjectName, string specialtyName)
        {
            if (string.IsNullOrEmpty(subjectName))
                throw new ArgumentException("Subject name cannot be empty.");

            if (string.IsNullOrEmpty(specialtyName))
                throw new ArgumentException("Specialty name cannot be empty.");

            // Get initials from subject name (up to 2 words)
            string subjectInitials = string.Concat(subjectName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Take(2)
                .Select(word => char.ToUpper(word[0])));

            if (string.IsNullOrEmpty(subjectInitials))
            {
                subjectInitials = "SB"; // Default if no valid initials
            }

            // Get initials from specialty name (up to 2 words)
            string specialtyInitials = string.Concat(specialtyName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
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
            var lastSubjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.GetAllAsync();
            var lastId = lastSubjectSpecialty
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

        public async Task<string> GenerateCourseId(string courseName, string level, string? relatedCourseId = null)
        {
            if (string.IsNullOrEmpty(courseName))
                throw new ArgumentException("Course name cannot be empty.");

            if (string.IsNullOrEmpty(level))
                throw new ArgumentException("Course level cannot be empty.");

            if (level == "Initial")
            {
                // Use initials extraction logic
                string courseInitials = string.Concat(courseName
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(word => !string.IsNullOrWhiteSpace(word))
                    .Take(3)
                    .Select(word => char.ToUpper(word[0])));

                if (string.IsNullOrEmpty(courseInitials))
                {
                    courseInitials = "CRS"; // Default if no valid initials
                }

                // Always start Initial with 101
                return $"{courseInitials}101";
            }

            if (string.IsNullOrEmpty(relatedCourseId))
                throw new ArgumentException("Related course ID is required for non-initial levels.");

            var relatedCourse = await _unitOfWork.CourseRepository.GetByIdAsync(relatedCourseId);
            if (relatedCourse == null)
                throw new ArgumentException("The related course does not exist.");

            // Extract initials (alphabetic part)
            string initialsPart = new(relatedCourse.CourseId.TakeWhile(char.IsLetter).ToArray());

            // Extract number (numeric part)
            string numberPart = new(relatedCourse.CourseId.SkipWhile(char.IsLetter).ToArray());

            if (!int.TryParse(numberPart, out int baseCode))
                throw new InvalidOperationException("Related course ID does not contain a valid numeric part.");

            // Determine new code based on level
            int newCode = level switch
            {
                "Professional" => baseCode + 100,
                "Recurrent" => baseCode + 10,
                "Relearn" => baseCode + 1,
                _ => throw new InvalidOperationException("Invalid CourseLevel.")
            };

            return $"{initialsPart}{newCode}";
        }
        #endregion

        #region Import Courses
        public async Task<ImportResult> ImportCoursesAsync(Stream excelStream, string importedByUserId)
        {
            var result = new ImportResult
            {
                SuccessCount = 0,
                FailedCount = 0,
                Errors = new List<string>()
            };

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(excelStream);
                var worksheet = package.Workbook.Worksheets[0]; // First worksheet

                // Adjusted column indexes:
                // Course Id = 2 (B), Course Name = 3 (C), Course Level = 4 (D), Specialty = 5 (E), Subjects start from 6 (F)
                int courseIdCol = 2;
                int courseNameCol = 3;
                int courseLevelCol = 4;
                int specialtyCol = 5;
                int subjectStartCol = 6;

                // Read subject names from row 8, starting from subjectStartCol (F)
                var subjectNames = new List<string>();
                int column = subjectStartCol;
                while (!string.IsNullOrWhiteSpace(worksheet.Cells[8, column].Text))
                {
                    subjectNames.Add(worksheet.Cells[8, column].Text.Trim());
                    column++;
                }

                // Validate subjects exist in the database
                var subjects = await _unitOfWork.SubjectRepository.GetAllAsync(
                    s => subjectNames.Contains(s.SubjectName));
                var subjectDict = subjects.ToDictionary(s => s.SubjectName, s => s);

                // Check for missing subjects and throw exception if any are not found
                var missingSubjects = subjectNames.Where(name => !subjectDict.ContainsKey(name)).ToList();
                if (missingSubjects.Any())
                {
                    throw new KeyNotFoundException($"The following subjects were not found in the database: {string.Join(", ", missingSubjects)}");
                }

                // Get all specialties that will be used
                var specialtyNames = new HashSet<string>();
                int row = 9;
                while (!string.IsNullOrWhiteSpace(worksheet.Cells[row, courseIdCol].Text))
                {
                    specialtyNames.Add(worksheet.Cells[row, specialtyCol].Text.Trim());
                    row++;
                }

                // Validate specialties exist
                var specialties = await _unitOfWork.SpecialtyRepository.GetAllAsync(
                    s => specialtyNames.Contains(s.SpecialtyName));
                var specialtyDict = specialties.ToDictionary(s => s.SpecialtyName, s => s);

                var missingSpecialties = specialtyNames.Where(name => !specialtyDict.ContainsKey(name)).ToList();
                if (missingSpecialties.Any())
                {
                    throw new KeyNotFoundException($"The following specialties were not found in the database: {string.Join(", ", missingSpecialties)}");
                }

                // Create a dictionary to store SubjectSpecialty records
                var subjectSpecialtyDict = new Dictionary<(string SubjectId, string SpecialtyId), SubjectSpecialty>();

                // Process courses starting from row 9
                row = 9;
                while (!string.IsNullOrWhiteSpace(worksheet.Cells[row, courseIdCol].Text))
                {
                    try
                    {
                        string relatedCourseId = worksheet.Cells[row, courseIdCol].Text.Trim();
                        string courseName = worksheet.Cells[row, courseNameCol].Text.Trim();
                        string courseLevel = worksheet.Cells[row, courseLevelCol].Text.Trim();
                        string specialtyName = worksheet.Cells[row, specialtyCol].Text.Trim();

                        string courseId;
                        if (courseLevel.Equals("Initial", StringComparison.OrdinalIgnoreCase))
                        {
                            // For Initial, use the Course Id from the Excel file directly
                            courseId = relatedCourseId;
                        }
                        else
                        {
                            // For other levels, use relatedCourseId to generate new courseId
                            courseId = await GenerateCourseId(courseName, courseLevel, relatedCourseId);
                        }

                        // Check if course already exists
                        var existingCourse = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
                        if (existingCourse != null)
                        {
                            result.Errors.Add($"Course with ID '{courseId}' already exists.");
                            result.FailedCount++;
                            row++;
                            continue;
                        }

                        var specialty = specialtyDict[specialtyName];

                        // Create new course
                        var course = new Course
                        {
                            CourseId = courseId,
                            CourseName = courseName,
                            CourseLevel = Enum.TryParse<CourseLevel>(courseLevel, true, out var levelEnum) ? levelEnum : CourseLevel.Initial,
                            Status = CourseStatus.Pending,
                            Progress = Progress.NotYet,
                            StartDateTime = DateTime.Now,
                            EndDateTime = DateTime.Now.AddDays(365),
                            CreatedByUserId = importedByUserId,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            SubjectSpecialties = new List<SubjectSpecialty>(),
                            RelatedCourseId = courseLevel.Equals("Initial", StringComparison.OrdinalIgnoreCase) ? null : relatedCourseId
                        };

                        // Save the course immediately so it can be referenced by subsequent rows
                        await _unitOfWork.CourseRepository.AddAsync(course);
                        await _unitOfWork.SaveChangesAsync();

                        // Process 'x' markers for subjects
                        for (int col = subjectStartCol, subjectIndex = 0; col < subjectStartCol + subjectNames.Count; col++, subjectIndex++)
                        {
                            if (worksheet.Cells[row, col].Text.Trim().ToLower() == "x")
                            {
                                var subject = subjectDict[subjectNames[subjectIndex]];
                                var key = (subject.SubjectId, specialty.SpecialtyId);

                                // Check if we already created this SubjectSpecialty in this import session
                                if (!subjectSpecialtyDict.TryGetValue(key, out var subjectSpecialty))
                                {
                                    // Check if SubjectSpecialty already exists in the database
                                    subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.FirstOrDefaultAsync(
                                        ss => ss.SubjectId == subject.SubjectId && ss.SpecialtyId == specialty.SpecialtyId);

                                    if (subjectSpecialty == null)
                                    {
                                        // Generate new SubjectSpecialty ID
                                        string subjectSpecialtyId = await GenerateSubjectSpecialtyId(subject.SubjectName, specialty.SpecialtyName);

                                        // Create new SubjectSpecialty
                                        subjectSpecialty = new SubjectSpecialty
                                        {
                                            SubjectSpecialtyId = subjectSpecialtyId,
                                            SubjectId = subject.SubjectId,
                                            SpecialtyId = specialty.SpecialtyId,
                                            Subject = subject,
                                            Specialty = specialty
                                        };
                                        await _unitOfWork.SubjectSpecialtyRepository.AddAsync(subjectSpecialty);
                                        await _unitOfWork.SaveChangesAsync(); // Save to get the ID
                                    }
                                    // Store in the import session dictionary for reuse
                                    subjectSpecialtyDict[key] = subjectSpecialty;
                                }

                                course.SubjectSpecialties.Add(subjectSpecialty);
                            }
                        }

                        // Save again if you added SubjectSpecialties
                        await _unitOfWork.CourseRepository.UpdateAsync(course);
                        await _unitOfWork.SaveChangesAsync();

                        // Create approval request for the imported course
                        var requestDto = new RequestDTO
                        {
                            RequestEntityId = courseId,
                            RequestType = RequestType.NewCourse,
                            Description = $"Request to approve imported course '{course.CourseName}'",
                            Notes = $"Course details:\n" +
                                   $"Level: {course.CourseLevel}\n" +
                                   $"Start Date: {course.StartDateTime}\n" +
                                   $"End Date: {course.EndDateTime}\n" +
                                   $"Specialty: {specialtyName}\n" +
                                   $"Subjects: {string.Join(", ", course.SubjectSpecialties.Select(ss => ss.Subject.SubjectName))}"
                        };
                        await _requestService.CreateRequestAsync(requestDto, importedByUserId);

                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error processing row {row}: {ex.Message}");
                        result.FailedCount++;
                    }

                    row++;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error importing courses: {ex.Message}");
                result.FailedCount++;
                return result;
            }
        }
        #endregion

        #region Assign SubjectSpecialty
        public async Task<CourseModel> AssignSubjectSpecialtyAsync(string courseId, string subjectSpecialtyId)
        {
            if (string.IsNullOrEmpty(courseId))
                throw new ArgumentException("Course ID cannot be empty.");

            if (string.IsNullOrEmpty(subjectSpecialtyId))
                throw new ArgumentException("SubjectSpecialty ID cannot be empty.");

            // Get course with its SubjectSpecialties
            var course = await _courseRepository.GetWithIncludesAsync(
                c => c.CourseId == courseId,
                query => query.Include(c => c.SubjectSpecialties)
            );

            if (course == null)
                throw new KeyNotFoundException($"Course with ID {courseId} does not exist.");

            // Get SubjectSpecialty with its Specialty
            var subjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.GetAsync(
                ss => ss.SubjectSpecialtyId == subjectSpecialtyId,
                ss => ss.Specialty
            );

            if (subjectSpecialty == null)
                throw new KeyNotFoundException($"SubjectSpecialty with ID {subjectSpecialtyId} does not exist.");

            // Check if the SubjectSpecialty is already assigned to the course
            if (course.SubjectSpecialties.Any(ss => ss.SubjectSpecialtyId == subjectSpecialtyId))
                throw new InvalidOperationException($"SubjectSpecialty {subjectSpecialtyId} is already assigned to course {courseId}.");

            // If course already has SubjectSpecialties, validate specialty match
            if (course.SubjectSpecialties.Any())
            {
                // Get the specialty of the first existing SubjectSpecialty
                var existingSubjectSpecialty = await _unitOfWork.SubjectSpecialtyRepository.GetAsync(
                    ss => ss.SubjectSpecialtyId == course.SubjectSpecialties.First().SubjectSpecialtyId,
                    ss => ss.Specialty
                );

                if (existingSubjectSpecialty == null)
                    throw new InvalidOperationException("Error retrieving existing SubjectSpecialty information.");

                // Compare specialties
                if (existingSubjectSpecialty.SpecialtyId != subjectSpecialty.SpecialtyId)
                    throw new InvalidOperationException($"Cannot assign SubjectSpecialty with specialty {subjectSpecialty.Specialty.SpecialtyName} to a course that already has subjects from specialty {existingSubjectSpecialty.Specialty.SpecialtyName}.");
            }

            // Add SubjectSpecialty to course
            course.SubjectSpecialties.Add(subjectSpecialty);
            course.UpdatedAt = DateTime.Now;

            // Update course in repository and save
            await _unitOfWork.CourseRepository.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CourseModel>(course);
        }
        #endregion
    }
}