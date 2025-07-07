using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PMGSupport.ThangTQ.Repositories.Basics;
using PMGSupport.ThangTQ.Repositories.DBContext;
using PMGSupport.ThangTQ.Repositories.Models;
using System.Linq.Expressions;
using System.Text;
using UglyToad.PdfPig;

namespace PMGSupport.ThangTQ.Repositories
{
    public class AssignmentRepository : GenericRepository<Assignment>
    {
        private new readonly SWD392Context _context;

        public AssignmentRepository() => _context ??= new SWD392Context();

        public AssignmentRepository(SWD392Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Assignment>?> GetAssignmentsAsync()
        {
            var assignments = await _context.Assignments.Include(a => a.Examiner).ToListAsync();
            return assignments;
        }

        public async Task<Assignment?> GetAssignmentByIdAsync(Guid id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Examiner)
                .FirstOrDefaultAsync(a => a.Id == id);
            return assignment;
        }

        public async Task<IEnumerable<Assignment>?> SearchAssignmentsAsync(string examinerId, DateTime uploadedAt, string status)
        {
            var assignments = await _context.Assignments.Include(a => a.Examiner)
                .Where(a => (string.IsNullOrEmpty(examinerId) || a.ExaminerId == examinerId) &&
                            (uploadedAt == default || a.UploadedAt.Date == uploadedAt.Date) &&
                            (string.IsNullOrEmpty(status) || a.Status == status))
                .ToListAsync();
            return assignments;
        }

        public async Task<bool> UploadBaremAsync(Guid assignmentId, string examinerId, IFormFile file, DateTime uploadedAt)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".pdf")
                {
                    return false;
                }
                var fileName = $"PMG201c_Barem_{examinerId}_{uploadedAt:ddMMyyyy_HHmmss}{extension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BaremFiles");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var assignment = await GetByIdAsync(assignmentId);
                if (assignment == null)
                {
                    return false;
                }
                assignment.BaremPath = filePath;
                assignment.Status = "Barem Uploaded";
                await UpdateAsync(assignment);
                await SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string ExtractTextFromPdf(string filePath)
        {
            var result = new StringBuilder();
            using (var document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    result.AppendLine(page.Text);
                }
            }
            return result.ToString();
        }

        public async Task<bool> UploadExamPaperAsync(string examinerId, IFormFile file, DateTime uploadedAt)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".jpg" && extension != ".png")
                {
                    return false;
                }

                var fileName = $"PMG201c_{examinerId}_{uploadedAt:ddMMyyyy_HHmmss}{extension}";
                var folderPath = Path.Combine("wwwroot", "ExamPapers");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var assignment = new Assignment
                {
                    Id = Guid.NewGuid(),
                    ExaminerId = examinerId,
                    UploadedAt = uploadedAt,
                    Status = "Uploaded",
                    FilePath = filePath,
                    BaremPath = string.Empty
                };

                await CreateAsync(assignment);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsByExaminerAsync(string examinerId)
        {
            return await _context.Assignments
                .Where(a => a.ExaminerId == examinerId)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();
        }

        public async Task<(string? ExamFilePath, string? BaremFilePath)> GetExamFilesByAssignmentIdAsync(Guid id)
        {
            var assignment = await GetAssignmentByIdAsync(id);
            if (assignment == null || string.IsNullOrEmpty(assignment.FilePath) || string.IsNullOrEmpty(assignment.BaremPath))
            {
                return (null, null);
            }

            return (assignment.FilePath, assignment.BaremPath);
        }
    }
}
