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
    public class AssignmentController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;

        public AssignmentController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider ?? throw new ArgumentNullException(nameof(servicesProvider));
        }

        [Authorize(Roles = "Examiner")]
        [HttpPost("upload-exam-paper")]
        public async Task<IActionResult> UploadExamPaper([FromForm] UploadExamPaperDTO uploadExamPaperDTO)
        {
            if (uploadExamPaperDTO.file == null || uploadExamPaperDTO.file.Length == 0)
            {
                return BadRequest("File is required.");
            }
            var examinerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(examinerId))
            {
                return Unauthorized("Examiner ID is required.");
            }

            var uploadedAt = DateTime.Now;
            var result = await _servicesProvider.AssignmentService.UploadExamPaperAsync(examinerId, uploadExamPaperDTO.file, uploadedAt);

            if (!result)
            {
                return StatusCode(500, "Upload failed");
            }

            return Ok("Upload successful");
        }

        [Authorize(Roles = "Examiner")]
        [HttpPost("upload-barem/{assignmentId}")]
        public async Task<IActionResult> UploadBarem([FromRoute] Guid assignmentId, [FromForm] UploadBaremDTO uploadBaremDTO)
        {
            if (uploadBaremDTO.file == null || uploadBaremDTO.file.Length == 0)
            {
                return BadRequest("File is required.");
            }
            var examinerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(examinerId))
            {
                return Unauthorized("Examiner ID is required.");
            }

            var assignment = await _servicesProvider.AssignmentService.GetAssignmentByIdAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound("Assignment not found.");
            }

            if (assignment.ExaminerId != examinerId)
            {
                return Forbid("You are not authorized to upload a barem for this assignment.");
            }

            var uploadedAt = DateTime.Now;
            var result = await _servicesProvider.AssignmentService.UploadBaremAsync(assignmentId, examinerId, uploadBaremDTO.file, uploadedAt);

            if (!result)
            {
                return StatusCode(500, "Upload failed");
            }
            return Ok(result);
        }

        [Authorize(Roles = "Examiner")]
        [HttpGet("assignments-examiner")]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignmentsByExaminerAsync()
        {
            var examinerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(examinerId))
            {
                return Unauthorized("Examiner ID is required.");
            }

            var assignments = await _servicesProvider.AssignmentService.GetAssignmentsByExaminerAsync(examinerId);
            return Ok(assignments);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("assignments-admin")]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignmentsAsync(int page = 1, int pageSize = 10, string? examniner = null, DateTime? uploadedAt = null, string? status = null)
        {
            var assignments = await _servicesProvider.AssignmentService.GetPagedAssignmentsAsync(page, pageSize, examniner, uploadedAt, status);
            if (assignments.Items == null || !assignments.Items.Any())
            {
                return NotFound("No assignments found.");
            }
            return Ok(new
            {
                Items = assignments.Items,
                TotalCount = assignments.TotalCount
            });
        }

        [Authorize(Roles = "Lecturer")]
        [HttpGet("download-exam-files/{id}")]
        public async Task<IActionResult> DownloadExamFilesAsync([FromRoute] Guid id)
        {
            var examFiles = await _servicesProvider.AssignmentService.GetExamFilesByAssignmentIdAsync(id);
            if (string.IsNullOrEmpty(examFiles.ExamFilePath) || string.IsNullOrEmpty(examFiles.BaremFilePath))
            {
                return NotFound("Exam paper or Barem not found for this assignment");
            }

            using var memoryStream = new MemoryStream();
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                if (System.IO.File.Exists(examFiles.ExamFilePath))
                {
                    var examPaperBytes = await System.IO.File.ReadAllBytesAsync(examFiles.ExamFilePath);
                    var zipEntry = zip.CreateEntry(Path.GetFileName(examFiles.ExamFilePath));
                    using var entryStream = zipEntry.Open();
                    await entryStream.WriteAsync(examPaperBytes);
                }

                if (System.IO.File.Exists(examFiles.BaremFilePath))
                {
                    var baremBytes = await System.IO.File.ReadAllBytesAsync(examFiles.BaremFilePath);
                    var zipEntry = zip.CreateEntry(Path.GetFileName(examFiles.BaremFilePath));
                    using var entryStream = zipEntry.Open();
                    await entryStream.WriteAsync(baremBytes);
                }
            }

            memoryStream.Position = 0;
            var zipFileName = $"Assignment_{id}.zip";
            return File(memoryStream.ToArray(), "application/zip", zipFileName);
        }

        [Authorize(Roles = "DepartmentLeader")]
        [HttpPost("assign-lecturers/{assignmentId}")]
        public async Task<IActionResult> AutoAssignLecturersAsync([FromRoute] Guid assignmentId)
        {
            if (assignmentId == Guid.Empty)
            {
                return BadRequest("Empty assignment id.");
            }
            var assignment = await _servicesProvider.AssignmentService.GetAssignmentByIdAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound("Not found assignment");
            }
            var departmentLeaderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(departmentLeaderId))
            {
                return Unauthorized("You must be logged in as Department Leader");
            }

            var result = await _servicesProvider.AssignmentService.AutoAssignLecturersAsync(departmentLeaderId, assignmentId);
            if (!result)
            {
                return BadRequest("No submissions or lecturers available.");
            }

            return Ok("Lecturers assigned successfully!");
        }
    }
}
