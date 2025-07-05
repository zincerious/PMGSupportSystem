using Microsoft.EntityFrameworkCore;
using PMGSupport.ThangTQ.Repositories.Basics;
using PMGSupport.ThangTQ.Repositories.DBContext;
using PMGSupport.ThangTQ.Repositories.Models;

namespace PMGSupport.ThangTQ.Repositories
{
    public class GradeRepository : GenericRepository<Grade>
    {
        private readonly new SWD392Context _context;
        public GradeRepository(SWD392Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Grade>> GetByAssignmentIdAsync(Guid assignmentId)
        {
            return await _context.Grades.Where(g => g.AssignmentId == assignmentId).ToListAsync();
        }
    }
}
