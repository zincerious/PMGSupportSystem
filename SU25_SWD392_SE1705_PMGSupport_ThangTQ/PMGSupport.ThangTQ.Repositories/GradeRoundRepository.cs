using Microsoft.EntityFrameworkCore;
using PMGSupport.ThangTQ.Repositories.Basics;
using PMGSupport.ThangTQ.Repositories.DBContext;
using PMGSupport.ThangTQ.Repositories.Models;

namespace PMGSupport.ThangTQ.Repositories
{
    public class GradeRoundRepository : GenericRepository<GradeRound>
    {
        private readonly new SWD392Context _context;
        public GradeRoundRepository(SWD392Context context)
        {
            _context = context;
        }

        public async Task<List<GradeRound>> GetByAssignmentIdAsync(Guid assignmentId)
        {
            return await _context.GradeRounds
                .Include(gr => gr.Grade)
                .Where(gr => gr.Grade.AssignmentId == assignmentId).ToListAsync();
        }

        public async Task<int> GetNextRoundNumberAsync(Guid assignmentId)
        {
            var gradeRounds = await _context.GradeRounds.Where(gr => gr.Grade.AssignmentId == assignmentId).ToListAsync();
            var maxRound = gradeRounds.Any() ? gradeRounds.Max(gr => gr.RoundNumber ?? 0) : 0;
            return maxRound + 1;
        }

        public async Task<GradeRound?> GetByGradeIdAndNumberAsync(Guid gradeId, int number)
        {
            return await _context.GradeRounds.FirstOrDefaultAsync(gr => gr.GradeId == gradeId && gr.RoundNumber == number);
        }

        public async Task AddRangeAsync(IEnumerable<GradeRound> gradeRounds)
        {
            await _context.GradeRounds.AddRangeAsync(gradeRounds);
        }
    }
}
