using Microsoft.Extensions.DependencyInjection;

namespace PMGSupport.ThangTQ.Services
{
    public interface  IServicesProvider
    {
        IAssignmentService AssignmentService { get; }
        IUserService UserService { get; }
        ISubmissionService SubmissionService { get; }
    }
    public class ServicesProvider : IServicesProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ServicesProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IAssignmentService AssignmentService => _serviceProvider.GetRequiredService<IAssignmentService>();
        public IUserService UserService => _serviceProvider.GetRequiredService<IUserService>();
        public ISubmissionService SubmissionService => _serviceProvider.GetRequiredService<ISubmissionService>();
    }
}
