using PMGSupport.ThangTQ.Repositories.Helpers;
using PMGSupport.ThangTQ.Repositories.DBContext;

namespace PMGSupport.ThangTQ.Repositories
{
    public interface IUnitOfWork
    {
        AssignmentRepository AssignmentRepository { get; }
        UserRepository UserRepository { get; }
        SubmissionRepository SubmissionRepository { get; }
        DistributionRepository DistributionRepository { get; }
        JwtHelper JwtHelper { get; }
        Task<int> SaveChangesAsync();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SWD392Context _context;
        private AssignmentRepository? _assignmentRepository;
        private UserRepository? _userRepository;
        private SubmissionRepository? _submissionRepository;
        private DistributionRepository? _distributionRepository;
        private readonly JwtHelper _jwtHelper;
        public UnitOfWork(SWD392Context context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }
        public AssignmentRepository AssignmentRepository
        {
            get
            {
                return _assignmentRepository ??= new AssignmentRepository(_context);
            }
        }
        public UserRepository UserRepository
        {
            get
            {
                return _userRepository ??= new UserRepository(_context);
            }
        }
        public SubmissionRepository SubmissionRepository
        {
            get
            {
                return _submissionRepository ??= new SubmissionRepository(_context);
            }
        }

        public DistributionRepository DistributionRepository
        {
            get
            {
                return _distributionRepository ??= new DistributionRepository(_context);
            }
        }
        public JwtHelper JwtHelper => _jwtHelper;

        public async Task<int> SaveChangesAsync()
        {
            int result = -1;

            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    result = await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception)
                {
                    await dbContextTransaction.RollbackAsync();
                }
            }

            return result;
        }

        public async Task Dispose()
        {
            await _context.DisposeAsync();
        }
    }
}
