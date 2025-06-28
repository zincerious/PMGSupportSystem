using System.ComponentModel.DataAnnotations;

namespace PMGSuppor.ThangTQ.Microservices.API.DTOs
{
    public class UploadStudentsSubmissionsDTO
    {
        [Required]
        public IFormFile ZipFile { get; set; } = null!;
    }
}
