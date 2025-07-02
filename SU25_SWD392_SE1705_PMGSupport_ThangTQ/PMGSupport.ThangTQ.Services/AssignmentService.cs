using Microsoft.AspNetCore.Http;
using PMGSupport.ThangTQ.Repositories;
using PMGSupport.ThangTQ.Repositories.Models;
using System.Linq.Expressions;

namespace PMGSupport.ThangTQ.Services
{
    public interface IAssignmentService
    {
        Task<IEnumerable<Assignment>> GetAssignmentsAsync();
        Task<Assignment?> GetAssignmentByIdAsync(Guid id);
        Task<IEnumerable<Assignment>?> SearchAssignmentsAsync(string examinerId, DateTime uploadedAt, string status);
        Task CreateAssignmentAsync(Assignment assignment);
        Task UpdateAssignmentAsync(Assignment assignment);
        Task DeleteAssignmentAsync(Assignment assignment);
        Task<(IEnumerable<Assignment> assignments, int totalCount)> GetAssignmentsWithPaginationAsync(int pageNumber, int pageSize, string? examninerId, DateTime? uploadedAt, string? status);
        Task<bool> UploadExamPaperAsync(string examinerId, IFormFile file, DateTime uploadedAt);
        Task<bool> UploadBaremAsync(Guid assignmentId, string examinerId, IFormFile file, DateTime uploadedAt);
        Task<IEnumerable<Assignment>> GetAssignmentsByExaminerAsync(string examinerId);
        Task<(IEnumerable<Assignment> Items, int TotalCount)> GetPagedAssignmentsAsync(int page, int pageSize, string? examinerId, DateTime? uploadedAt, string? status);
        Task<(string? ExamFilePath, string? BaremFilePath)> GetExamFilesByAssignmentIdAsync(Guid id);
        Task<bool> AutoAssignLecturersAsync(string assignedByUserId, Guid assignmentId);
    }
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public AssignmentService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task CreateAssignmentAsync(Assignment assignment)
        {
            await _unitOfWork.AssignmentRepository.CreateAsync(assignment);
        }

        public async Task DeleteAssignmentAsync(Assignment assignment)
        {
            await _unitOfWork.AssignmentRepository.DeleteAsync(assignment);
        }

        public async Task<Assignment?> GetAssignmentByIdAsync(Guid id)
        {
            return await _unitOfWork.AssignmentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsAsync()
        {
            return await _unitOfWork.AssignmentRepository.GetAllAsync();
        }

        public async Task<(IEnumerable<Assignment> assignments, int totalCount)> GetAssignmentsWithPaginationAsync(int pageNumber, int pageSize, string? examninerId, DateTime? uploadedAt, string? status)
        {
            Expression<Func<Assignment, bool>>? filter = null;

            filter = x =>
                (string.IsNullOrEmpty(examninerId) || x.ExaminerId == examninerId) &&
                (!uploadedAt.HasValue || x.UploadedAt.Date == uploadedAt.Value.Date) &&
                (string.IsNullOrEmpty(status) || x.Status == status);

            return await _unitOfWork.AssignmentRepository.GetPagedListAsync(
                page: pageNumber,
                pageSize: pageSize,
                filter: filter,
                q => q.OrderBy(x => x.Id));
        }

        public async Task<IEnumerable<Assignment>?> SearchAssignmentsAsync(string examinerId, DateTime uploadedAt, string status)
        {
            return await _unitOfWork.AssignmentRepository.SearchAssignmentsAsync(examinerId, uploadedAt, status);
        }

        public async Task UpdateAssignmentAsync(Assignment assignment)
        {
            await _unitOfWork.AssignmentRepository.UpdateAsync(assignment);
        }

        public async Task<bool> UploadExamPaperAsync(string examinerId, IFormFile file, DateTime uploadedAt)
        {
            return await _unitOfWork.AssignmentRepository.UploadExamPaperAsync(examinerId, file, uploadedAt);
        }

        public async Task<bool> UploadBaremAsync(Guid assignmentId, string examinerId, IFormFile file, DateTime uploadedAt)
        {
            return await _unitOfWork.AssignmentRepository.UploadBaremAsync(assignmentId, examinerId, file, uploadedAt);
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsByExaminerAsync(string examinerId)
        {
            return await _unitOfWork.AssignmentRepository.GetAssignmentsByExaminerAsync(examinerId);
        }

        public async Task<(IEnumerable<Assignment> Items, int TotalCount)> GetPagedAssignmentsAsync(int page, int pageSize, string? examinerId, DateTime? uploadedAt, string? status)
        {
            Expression<Func<Assignment, bool>>? filter = null;

            if (!string.IsNullOrEmpty(examinerId) || uploadedAt.HasValue || !string.IsNullOrEmpty(status))
            {
                filter = x =>
                    (string.IsNullOrEmpty(examinerId) || x.ExaminerId == examinerId) &&
                    (!uploadedAt.HasValue || x.UploadedAt.Date == uploadedAt.Value.Date) &&
                    (string.IsNullOrEmpty(status) || x.Status == status);
            }

            var assignments = await _unitOfWork.AssignmentRepository.GetPagedListAsync(
                page: page,
                pageSize: pageSize,
                filter: filter,
                q => q.OrderBy(x => x.Id));

            return assignments;
        }

        public async Task<(string? ExamFilePath, string? BaremFilePath)> GetExamFilesByAssignmentIdAsync(Guid id)
        {
            return await _unitOfWork.AssignmentRepository.GetExamFilesByAssignmentIdAsync(id);
        }

        public async Task<bool> AutoAssignLecturersAsync(string assignedByUserId, Guid assignmentId)
        {
            var submissions = await _unitOfWork.SubmissionRepository.GetAllAsync();
            var distributions = await _unitOfWork.DistributionRepository.GetAllAsync();

            var unassignedSubmission = submissions.Where(s => s.AssignmentId == assignmentId && 
                                        !distributions.Any(d => d.AssignmentId == assignmentId &&
                                        d.StudentId == s.StudentId)).ToList();
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            var lecturers = users.Where(u => u.Role == "Lecturer").ToList();

            if (!lecturers.Any() || !unassignedSubmission.Any())
            {
                return false;
            }

            var now = DateTime.Now;
            var newDistribution = new List<AssignmentDistribution>();

            for (int i = 0; i < unassignedSubmission.Count(); i++)
            {
                var submission = unassignedSubmission[i];
                int j = i % lecturers.Count();
                var lecturer = lecturers[j];

                var assignmentDistribution = new AssignmentDistribution
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = assignmentId,
                    AssignedBy = assignedByUserId,
                    LecturerId = lecturer.Id,
                    AssignedAt = now,
                    StudentId = submission.StudentId,
                    UpdatedAt = now,
                };

                newDistribution.Add(assignmentDistribution);
            }

            await _unitOfWork.DistributionRepository.AddRangeAsync(newDistribution);
            await _unitOfWork.SaveChangesAsync();

            var lecturerAssignments = newDistribution.GroupBy(d => d.LecturerId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var lecturerId in lecturerAssignments.Keys)
            {
                var lecturer = lecturers.FirstOrDefault(l => l.Id == lecturerId);
                if (lecturer != null)
                {
                    var assignments = lecturerAssignments[lecturerId];
                    var listStudents = string.Join("<br/>", assignments.Select(a =>
                    {
                        var student = users.FirstOrDefault(u => u.Id == a.StudentId);
                        return $"- {student?.FullName} - {student?.Id}";
                    }));

                    var subject = "New Grading Assignments";
                    var body = $"Dear {lecturer.FullName},<br/>" +
                               $"You have been assigned to review the following students:<br/>{listStudents}<br/>" +
                               $"Please login to the system to start grading.";

                    await _emailService.SendMailAsync(lecturer.Email, subject, body);
                }
            }

            return true;
        }
    }
}
