using Microsoft.AspNetCore.Mvc;
using PMGSupport.ThangTQ.Services;

namespace PMGSuppor.ThangTQ.Microservices.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class GradeController : ControllerBase
{
    private readonly IGradeService _gradeService;

    public GradeController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    [HttpGet("get-grades")]
    public async Task<IActionResult> GetGrades()
    {
        var a = await _gradeService.GetAllGradesAsync();
        if (a != null)
            return Ok(a);

        return NotFound("No grades found.");
    }

}