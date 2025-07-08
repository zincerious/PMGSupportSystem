using Microsoft.EntityFrameworkCore;
using PMGSupport.ThangTQ.Repositories.Basics;
using PMGSupport.ThangTQ.Repositories.DBContext;
using PMGSupport.ThangTQ.Repositories.Models;

namespace PMGSupport.ThangTQ.Repositories;

public class GradeRepository : GenericRepository<Grade>
{
    private readonly SWD392Context _context;

    public GradeRepository(SWD392Context context) : base(context)
    {
        _context = context;
    }

    public async Task<Grade> GetByAssigmentAndStudentAsync(Guid assigmentId, string studentId)
    {
        return await _context.Grades.FirstOrDefaultAsync(g => g.AssignmentId == assigmentId && g.StudentId == studentId);
    }
}