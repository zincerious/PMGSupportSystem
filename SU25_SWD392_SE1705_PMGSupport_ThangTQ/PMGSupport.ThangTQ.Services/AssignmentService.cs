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
    }
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AssignmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task CreateAssignmentAsync(Assignment assignment)
        {
            await _unitOfWork.AssignmentRepository.CreateAsync(assignment);
        }

        public Task UploadExamPaper()
        {
            throw new Exception();
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
    }
}
