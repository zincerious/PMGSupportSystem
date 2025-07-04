﻿using Microsoft.EntityFrameworkCore;
using PMGSupport.ThangTQ.Repositories.Basics;
using PMGSupport.ThangTQ.Repositories.DBContext;
using PMGSupport.ThangTQ.Repositories.Models;

namespace PMGSupport.ThangTQ.Repositories
{
    public class DistributionRepository : GenericRepository<AssignmentDistribution>
    {
        private new readonly SWD392Context _context;
        public DistributionRepository() => _context ??= new SWD392Context();
        public DistributionRepository(SWD392Context context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AssignmentDistribution>> GetDistributionsByAssignmentIdAsync(Guid assignmentId)
        {
            return await _context.AssignmentDistributions
                .Where(d => d.AssignmentId == assignmentId)
                .Include(d => d.Lecturer)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentDistribution>> GetDistributionsByLecturerIdAsync(string lecturerId)
        {
            return await _context.AssignmentDistributions
                .Where(d => d.LecturerId == lecturerId)
                .Include(d => d.Lecturer)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssignmentDistribution>> GetDistributionsByLecturerAndAssignment(Guid assignmentId, string lecturerId)
        {
            return await _context.AssignmentDistributions
                .Where(d => d.AssignmentId == assignmentId && d.LecturerId == lecturerId)
                .Include(d => d.Lecturer)
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<AssignmentDistribution> distributions)
        {
            foreach (var distribution in distributions)
            {
                await _context.AssignmentDistributions.AddAsync(distribution);
            }
        }
    }
}
