using System.ComponentModel.DataAnnotations;

namespace PMGSuppor.ThangTQ.Microservices.API.DTOs
{
    public class NewUsersDTO
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
