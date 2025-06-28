using System.ComponentModel.DataAnnotations;

namespace PMGSuppor.ThangTQ.Microservices.API.DTOs
{
    public class UploadBaremDTO
    {
        [Required]
        public IFormFile file { get; set; } = null!;
    }
}
