using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMGSuppor.ThangTQ.Microservices.API.DTOs;
using PMGSupport.ThangTQ.Services;
using System.Security.Claims;

namespace PMGSuppor.ThangTQ.Microservices.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public SubmissionController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        [Authorize(Roles = "Examiner")]
        [HttpPost("upload-submission/{assignmentId}")]
        public async Task<IActionResult> UploadSubmission([FromRoute] Guid assignmentId, [FromForm] UploadStudentsSubmissionsDTO dto)
        {
            if (dto.ZipFile == null || dto.ZipFile.Length == 0)
            {
                return BadRequest("Zip file is required.");
            }

            var examinerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(examinerId))
            {
                return Unauthorized("Examiner ID is required.");
            }

            var examiner = await _servicesProvider.UserService.GetUserByIdAsync(examinerId);
            if (examiner == null)
            {
                return NotFound("Examiner not found.");
            }

            var result = await _servicesProvider.SubmissionService.UploadSubmissionsAsync(assignmentId, dto.ZipFile, examinerId);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload submissions.");
            }

            return Ok("Submissions uploaded successfully.");
        }
    }
}
