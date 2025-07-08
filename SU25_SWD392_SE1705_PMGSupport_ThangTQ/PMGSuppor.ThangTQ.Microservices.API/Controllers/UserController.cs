using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMGSuppor.ThangTQ.Microservices.API.DTOs;
using PMGSupport.ThangTQ.Repositories.Models;
using PMGSupport.ThangTQ.Services;

namespace PMGSuppor.ThangTQ.Microservices.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public UserController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<User>>> GetAllUsersAsync()
        //{
        //    var users = await _servicesProvider.UserService.GetUsersAsync();
        //    return Ok(users);
        //}

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] string idToken)
        {
            var jwt = await _servicesProvider.UserService.LoginAsync(idToken);
            return Ok(jwt);
        }

        [Authorize]
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return Ok(new { Message = "Logout successful", Token = token, RevokedAt = DateTime.Now });
        }

        [Authorize(Roles = "Admin, Examiner")]
        [HttpPost("import-users")]
        public async Task<IActionResult> ImportUsersAsync(NewUsersDTO newUsersDTO)
        {
            if (newUsersDTO.File == null || newUsersDTO.File.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var stream = newUsersDTO.File.OpenReadStream())
            {
                var importedUsers = await _servicesProvider.UserService.ImportUsersFromExcelAsync(stream);
                return Ok(new { Message = "Users imported successfully!", Count = importedUsers.Count() });
            }
        }
    }
}
