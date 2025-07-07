using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMGSuppor.ThangTQ.Microservices.API.DTOs;
using PMGSupport.ThangTQ.Repositories.Models;
using PMGSupport.ThangTQ.Services;
using System.IO.Compression;
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

        [Authorize(Roles = "Lecturer")]
        [HttpGet("download-submissions/{assignmentId}")]
        public async Task<IActionResult> DownloadSubmissionsAsync([FromRoute] Guid assignmentId)
        {
            if (assignmentId == Guid.Empty)
            {
                return BadRequest("Empty assignment id.");
            }

            var assignment = await _servicesProvider.AssignmentService.GetAssignmentByIdAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound("Not found assignment.");
            }

            var lecturerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(lecturerId))
            {
                return Unauthorized("Not lecturer role.");
            }

            var distributions = await _servicesProvider.DistributionService.GetDistributionsByLecturerIdAndAssignmentIdAsync(assignmentId, lecturerId);
            if (distributions == null || !distributions.Any())
            {
                return NotFound("Not found distributions.");
            }

            var studentIds = distributions.Select(d => d.StudentId).ToList();

            var submissions = await _servicesProvider.SubmissionService.GetSubmissionsByAssignmentAndStudentsAsync(assignmentId, studentIds);
            if (submissions == null || !submissions.Any())
            {
                return NotFound("Not found submissions.");
            }

            using var memoryStream = new MemoryStream();
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var submission in submissions)
                {
                    if (System.IO.File.Exists(submission.FilePath))
                    {
                        var fileBytes = await System.IO.File.ReadAllBytesAsync(submission.FilePath);
                        var studentName = submission.Student.FullName ?? "Unknown";
                        var fileNameInZip = $"{studentName}_{submission.StudentId}{Path.GetExtension(submission.FilePath)}";

                        var zipEntry =  zip.CreateEntry(fileNameInZip);
                        using var entryStream = zipEntry.Open();
                        await entryStream.WriteAsync(fileBytes);
                    }
                }
            }

            memoryStream.Position = 0;
            var zipFileName = $"Submissions_{assignmentId}.zip";
            return File(memoryStream.ToArray(), "application/zip", zipFileName);
        }
    }
}
