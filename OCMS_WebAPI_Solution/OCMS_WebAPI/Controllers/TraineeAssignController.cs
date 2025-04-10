using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_BOs.ResponseModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TraineeAssignController : ControllerBase
    {
        private readonly ITraineeAssignService _traineeAssignService;
        private readonly IBlobService _blobService;

        public TraineeAssignController(ITraineeAssignService traineeAssignService, IBlobService blobService)
        {
            _traineeAssignService = traineeAssignService;
            _blobService = blobService;
        }

        #region Import Trainee Assignments
        [HttpPost("import")]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> ImportTraineeAssignments(IFormFile file)
        {
            var importedByUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    ImportResult result = await _traineeAssignService.ImportTraineeAssignmentsFromExcelAsync(stream, importedByUserId);

                    if (result.Errors.Count > 0)
                    {
                        return Ok(new { Message = "Import completed with errors.", Result = result });
                    }

                    return Ok(new { Message = "Import successful.", Result = result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        #region Create Trainee Assignment
        [HttpPost]
        [CustomAuthorize("Admin", "Training staff")]
        public async Task<IActionResult> CreateTraineeAssignment([FromBody] TraineeAssignDTO dto)
        {
            
                try
                {
                    var createdByUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    var traineeAssignment = await _traineeAssignService.CreateTraineeAssignAsync(dto, createdByUserId);
                    return Ok(traineeAssignment);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            #endregion

            #region Get All Trainee Assignments
            [HttpGet]
            [CustomAuthorize("Admin", "Training staff", "Reviewer")]
            public async Task<IActionResult> GetAllTraineeAssignments()
            {
                try
                {
                    var traineeAssignments = await _traineeAssignService.GetAllTraineeAssignmentsAsync();
                    return Ok(traineeAssignments);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }
            #endregion

            #region Get Trainee Assignment By Id
            [HttpGet("{id}")]
            [CustomAuthorize("Admin", "Training staff", "Reviewer")]
            public async Task<IActionResult> GetTraineeAssignmentById(string id)
            {
                try
                {
                    var traineeAssignment = await _traineeAssignService.GetTraineeAssignmentByIdAsync(id);
                    return Ok(traineeAssignment);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }
            #endregion

            #region Update Trainee Assignment
            [HttpPut("{id}")]
            [CustomAuthorize("Admin", "Training staff")]
            public async Task<IActionResult> UpdateTraineeAssignment(string id, [FromBody] TraineeAssignDTO updatedTraineeAssign)
            {
                try
                {
                    var traineeAssignment = await _traineeAssignService.UpdateTraineeAssignmentAsync(id, updatedTraineeAssign);
                    return Ok(traineeAssignment);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }
            #endregion

            #region Delete Trainee Assignment
            [HttpDelete("{id}")]
            [CustomAuthorize("Admin", "Training staff")]
            public async Task<IActionResult> DeleteTraineeAssignment(string id)
            {
                if (string.IsNullOrEmpty(id)) 
                {
                    return BadRequest("Invalid ID");
                }

                var (success, message) = await _traineeAssignService.DeleteTraineeAssignmentAsync(id);

                if (!success)
                {
                    return BadRequest(message);
                }

                return Ok(new { Success = success, Message = message });
            }
        #endregion

        [HttpGet("trainee/{traineeId}/courses")]
        [CustomAuthorize("Trainee")]
        public async Task<IActionResult> GetCoursesByTraineeId(string traineeId)
        {
            try
            {
                var courses = await _traineeAssignService.GetCoursesByTraineeIdAsync(traineeId);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("subject/{subjectId}/trainees")]
        [CustomAuthorize("Admin", "Instructor")]
        public async Task<IActionResult> GetTraineesBySubjectId(string subjectId)
        {
            try
            {
                var trainees = await _traineeAssignService.GetTraineesBySubjectIdAsync(subjectId);
                return Ok(trainees);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }


    }

}

