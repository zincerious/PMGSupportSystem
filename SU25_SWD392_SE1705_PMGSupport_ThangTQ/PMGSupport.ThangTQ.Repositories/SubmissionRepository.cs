using Microsoft.EntityFrameworkCore;
using PMGSupport.ThangTQ.Repositories.Basics;
using PMGSupport.ThangTQ.Repositories.DBContext;
using PMGSupport.ThangTQ.Repositories.Models;

namespace PMGSupport.ThangTQ.Repositories
{
    public class SubmissionRepository : GenericRepository<Submission>
    {
        private new readonly SWD392Context _context;
        public SubmissionRepository() => _context ??= new SWD392Context();
        public SubmissionRepository(SWD392Context context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<Submission> submissions)
        {
            await _context.Submissions.AddRangeAsync(submissions);
            await _context.SaveChangesAsync();
        }   

        public async Task<Submission?> GetSubmissionByStudentIdAsync(string studentId)
        {
            return await _context.Submissions
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.StudentId == studentId);
        }

        public async Task<IEnumerable<Submission>?> GetSubmissionsAsync()
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .ToListAsync();
        }

        public async Task<IEnumerable<Submission>?> GetSubmissionsByAssignmentIdAsync(Guid assignmentId)
        {
            return await _context.Submissions
                .Where(s => s.AssignmentId == assignmentId)
                .Include(s => s.Student)
                .ToListAsync();
        }

        public async Task<IEnumerable<Submission>?> GetSubmissionsByAssignmentAndStudentsAsync(Guid assignmentId, IEnumerable<string> studentIds)
        {
            return await _context.Submissions
                .Where(s => s.AssignmentId == assignmentId && studentIds.Contains(s.StudentId))
                .Include(s => s.Student)
                .ToListAsync();
        }

        public async Task<Submission?> GetSubmissionByIdAsync(Guid id)
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Submission>> GetSubmissionsForNextRoundAsync(Guid assignmentId, int roundNumber)
        {
            var submissions = await _context.Submissions.Include(s => s.Student).Where(s => s.AssignmentId == assignmentId).ToListAsync();

            if (roundNumber == 1)
            {
                return submissions.Where(s =>
                    !_context.GradeRounds.Any(gr =>
                            gr.Grade.AssignmentId == assignmentId && gr.Grade.StudentId == s.StudentId)).ToList();
            }
            else
            {
                return submissions.Where(s =>
                    _context.GradeRounds.Any(gr => 
                        gr.Grade.AssignmentId == assignmentId
                        && gr.Grade.StudentId == s.StudentId
                        && gr.RoundNumber == (roundNumber - 1))).ToList();
            }
        }

    }
}
