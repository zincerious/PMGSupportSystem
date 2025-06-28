using Google.Apis.Auth;
using PMGSupport.ThangTQ.Repositories;
using PMGSupport.ThangTQ.Repositories.Models;

namespace PMGSupport.ThangTQ.Services
{
    public interface IUserService
    {
        Task<string> LoginAsync(string email);
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersAsync();
    }
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(email);
            return user;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            return users;
        }

        public async Task<string> LoginAsync(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            var googleId = payload.Subject;
            var email = payload.Email;

            var user = await _unitOfWork.UserRepository.GetByGoogleIdAsync(googleId);
            if (user == null)
            {
                user = await _unitOfWork.UserRepository.GetByEmailAsync(email);

                if (user == null) throw new UnauthorizedAccessException("Unexisted user!");

                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleId;
                    await _unitOfWork.UserRepository.UpdateAsync(user);
                }
                else
                {
                    if (user.GoogleId != googleId)
                    {
                        throw new UnauthorizedAccessException("Google ID mismatch!");
                    }
                }
            }
            if (user.Email != email)
            {
                user.Email = email;
                await _unitOfWork.UserRepository.UpdateAsync(user);
            }

            var jwt = _unitOfWork.JwtHelper.GenerateToken(user);
            return jwt;
        }
    }
}
