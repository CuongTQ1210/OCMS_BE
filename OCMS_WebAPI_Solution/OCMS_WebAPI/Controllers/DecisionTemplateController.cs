using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCMS_BOs.RequestModel;
using OCMS_Services.IService;
using OCMS_WebAPI.AuthorizeSettings;
using System.Security.Claims;

namespace OCMS_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DecisionTemplateController : ControllerBase
    {
        private readonly IDecisionTemplateService _decisionTemplateService;
        private readonly IBlobService _blobService;

        public DecisionTemplateController(IDecisionTemplateService decisionTemplateService, IBlobService blobService)
        {
            _decisionTemplateService = decisionTemplateService;
            _blobService = blobService;
        }

        #region Create Decision Template
        [HttpPost("Create")]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> CreateDecisionTemplate([FromForm] CreateDecisionTemplateDTO dto)
        {
            if (dto == null)
                return BadRequest("Decision template data cannot be null");
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _decisionTemplateService.CreateDecisionTemplateAsync(dto, currentUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        #region Get All Decision Templates
        [HttpGet("GetAll")]
        [CustomAuthorize("Admin", "HeadMaster")]
        public async Task<IActionResult> GetAllDecisionTemplates()
        {
            try
            {
                var templates = await _decisionTemplateService.GetAllDecisionTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        #region Get Decision Template By Id
        [HttpGet("GetById/{templateId}")]
        [CustomAuthorize("Admin", "HeadMaster")]
        public async Task<IActionResult> GetDecisionTemplateById(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
                return BadRequest("Template ID cannot be null or empty");
            try
            {
                var template = await _decisionTemplateService.GetDecisionTemplateDetailsByIdAsync(templateId);
                if (template == null)
                    return NotFound($"Template with ID {templateId} not found");

                return Ok(template);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        #region Get Decision Template By Name
        [HttpGet("GetByName/{templateName}")]
        [CustomAuthorize("Admin", "HeadMaster")]
        public async Task<IActionResult> GetDecisionTemplateByName(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
                return BadRequest("Template name cannot be null or empty");
            try
            {
                var template = await _decisionTemplateService.GetDecisionTemplateByNameAsync(templateName);
                if (template == null)
                    return NotFound($"Template with name {templateName} not found");
                return Ok(template);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion        

        #region Update Decision Template
        [HttpPut("Update/{templateId}")]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> UpdateDecisionTemplate(string templateId, [FromForm] UpdateDecisionTemplateDTO dto)
        {
            if (string.IsNullOrEmpty(templateId))
                return BadRequest("Template ID cannot be null or empty");
            if (dto == null)
                return BadRequest("Decision template data cannot be null");
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _decisionTemplateService.UpdateDecisionTemplateAsync(templateId, dto, currentUserId);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        #region Delete Decision Template
        [HttpDelete("Delete/{templateId}")]
        [CustomAuthorize("Admin")]
        public async Task<IActionResult> DeleteDecisionTemplate(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
                return BadRequest("Template ID cannot be null or empty");
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await _decisionTemplateService.DeleteDecisionTemplateAsync(templateId, currentUserId);
                if (!result)
                    return NotFound($"Template with ID {templateId} not found");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion
    }
}
