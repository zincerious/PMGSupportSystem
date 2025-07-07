using PMGSupport.ThangTQ.Repositories;
using PMGSupport.ThangTQ.Repositories.Models;

namespace PMGSupport.ThangTQ.Services
{
    public interface IDistributionService
    {
        Task<IEnumerable<AssignmentDistribution>> GetDistributionsAsync();
        Task<IEnumerable<AssignmentDistribution>> GetDistributionsByLecturerIdAndAssignmentIdAsync(Guid assignmentId, string lecturerId);
    }
    public class DistributionService : IDistributionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DistributionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AssignmentDistribution>> GetDistributionsAsync()
        {
            return await _unitOfWork.DistributionRepository.GetAllAsync();
        }

        public async Task<IEnumerable<AssignmentDistribution>> GetDistributionsByLecturerIdAndAssignmentIdAsync(Guid assignmentId, string lecturerId)
        {
            return await _unitOfWork.DistributionRepository.GetDistributionsByLecturerAndAssignment(assignmentId, lecturerId);
        }
    }
}
