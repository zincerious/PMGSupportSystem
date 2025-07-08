using PMGSupport.ThangTQ.Repositories;
using PMGSupport.ThangTQ.Repositories.Models;
using PMGSupport.ThangTQ.Services.DTOs;

namespace PMGSupport.ThangTQ.Services;

public interface IGradeService
{
    Task<IEnumerable<Grade>> GetAllGradesAsync();
    Task<ResultDTO<Grade>> GetGradebyStudentAndAsignment(GradeDTOSearch search);
}
public class GradeService : IGradeService
{
    private readonly IUnitOfWork _unitOfWork;

    public GradeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public Task<IEnumerable<Grade>> GetAllGradesAsync()
    {
       var grade = _unitOfWork.GradeRepository.GetAllAsync();
        return grade; 
    }

    public async Task<ResultDTO<Grade>> GetGradebyStudentAndAsignment(GradeDTOSearch search)
    {
        
        var grade = await _unitOfWork.GradeRepository.GetByAssigmentAndStudentAsync(search.AssignmentId, search.StudentId);
        if (grade == null) return ResultDTO<Grade>.Fail("No grade found", 400);
        return ResultDTO<Grade>.Ok(grade, 200,"Success");
    }


}