namespace PMGSupport.ThangTQ.Services.DTOs;

public class GradeDTO
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }
    public string AssignmentName { get; set; }
    public string StudentId { get; set; }
    public string StudentName { get; set; }
    public double? FinalScore { get; set; }
}

public class GradeDTOSearch
{
    public string StudentId { get; set; }
    public Guid AssignmentId { get; set; }
}