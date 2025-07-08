using Microsoft.AspNetCore.Http;
using PMGSupport.ThangTQ.Repositories;
using PMGSupport.ThangTQ.Repositories.Models;
using System.Linq.Expressions;

namespace PMGSupport.ThangTQ.Services
{
    public interface IAssignmentService
    {
        Task<IEnumerable<Assignment>> GetAssignmentsAsync();
        Task<Assignment?> GetAssignmentByIdAsync(Guid id);
        Task<IEnumerable<Assignment>?> SearchAssignmentsAsync(string examinerId, DateTime uploadedAt, string status);
        Task CreateAssignmentAsync(Assignment assignment);
        Task UpdateAssignmentAsync(Assignment assignment);
        Task DeleteAssignmentAsync(Assignment assignment);
        Task<(IEnumerable<Assignment> assignments, int totalCount)> GetAssignmentsWithPaginationAsync(int pageNumber, int pageSize, string? examninerId, DateTime? uploadedAt, string? status);
        Task<bool> UploadExamPaperAsync(string examinerId, IFormFile file, DateTime uploadedAt);
        Task<bool> UploadBaremAsync(Guid assignmentId, string examinerId, IFormFile file, DateTime uploadedAt);
        Task<IEnumerable<Assignment>> GetAssignmentsByExaminerAsync(string examinerId);
        Task<(IEnumerable<Assignment> Items, int TotalCount)> GetPagedAssignmentsAsync(int page, int pageSize, string? examinerId, DateTime? uploadedAt, string? status);
        Task<(string? ExamFilePath, string? BaremFilePath)> GetExamFilesByAssignmentIdAsync(Guid id);
        Task<bool> AutoAssignLecturersAsync(string assignedByUserId, Guid assignmentId);
    }
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public AssignmentService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task CreateAssignmentAsync(Assignment assignment)
        {
            await _unitOfWork.AssignmentRepository.CreateAsync(assignment);
        }

        public async Task DeleteAssignmentAsync(Assignment assignment)
        {
            await _unitOfWork.AssignmentRepository.DeleteAsync(assignment);
        }

        public async Task<Assignment?> GetAssignmentByIdAsync(Guid id)
        {
            return await _unitOfWork.AssignmentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsAsync()
        {
            return await _unitOfWork.AssignmentRepository.GetAllAsync();
        }

        public async Task<(IEnumerable<Assignment> assignments, int totalCount)> GetAssignmentsWithPaginationAsync(int pageNumber, int pageSize, string? examninerId, DateTime? uploadedAt, string? status)
        {
            Expression<Func<Assignment, bool>>? filter = null;

            filter = x =>
                (string.IsNullOrEmpty(examninerId) || x.ExaminerId == examninerId) &&
                (!uploadedAt.HasValue || x.UploadedAt.Date == uploadedAt.Value.Date) &&
                (string.IsNullOrEmpty(status) || x.Status == status);

            return await _unitOfWork.AssignmentRepository.GetPagedListAsync(
                page: pageNumber,
                pageSize: pageSize,
                filter: filter,
                q => q.OrderBy(x => x.Id));
        }

        public async Task<IEnumerable<Assignment>?> SearchAssignmentsAsync(string examinerId, DateTime uploadedAt, string status)
        {
            return await _unitOfWork.AssignmentRepository.SearchAssignmentsAsync(examinerId, uploadedAt, status);
        }

        public async Task UpdateAssignmentAsync(Assignment assignment)
        {
            await _unitOfWork.AssignmentRepository.UpdateAsync(assignment);
        }

        public async Task<bool> UploadExamPaperAsync(string examinerId, IFormFile file, DateTime uploadedAt)
        {
            return await _unitOfWork.AssignmentRepository.UploadExamPaperAsync(examinerId, file, uploadedAt);
        }

        public async Task<bool> UploadBaremAsync(Guid assignmentId, string examinerId, IFormFile file, DateTime uploadedAt)
        {
            return await _unitOfWork.AssignmentRepository.UploadBaremAsync(assignmentId, examinerId, file, uploadedAt);
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsByExaminerAsync(string examinerId)
        {
            return await _unitOfWork.AssignmentRepository.GetAssignmentsByExaminerAsync(examinerId);
        }

        public async Task<(IEnumerable<Assignment> Items, int TotalCount)> GetPagedAssignmentsAsync(int page, int pageSize, string? examinerId, DateTime? uploadedAt, string? status)
        {
            Expression<Func<Assignment, bool>>? filter = null;

            if (!string.IsNullOrEmpty(examinerId) || uploadedAt.HasValue || !string.IsNullOrEmpty(status))
            {
                filter = x =>
                    (string.IsNullOrEmpty(examinerId) || x.ExaminerId == examinerId) &&
                    (!uploadedAt.HasValue || x.UploadedAt.Date == uploadedAt.Value.Date) &&
                    (string.IsNullOrEmpty(status) || x.Status == status);
            }

            var assignments = await _unitOfWork.AssignmentRepository.GetPagedListAsync(
                page: page,
                pageSize: pageSize,
                filter: filter,
                q => q.OrderBy(x => x.Id));

            return assignments;
        }

        public async Task<(string? ExamFilePath, string? BaremFilePath)> GetExamFilesByAssignmentIdAsync(Guid id)
        {
            return await _unitOfWork.AssignmentRepository.GetExamFilesByAssignmentIdAsync(id);
        }

        public async Task<bool> AutoAssignLecturersAsync(string assignedByUserId, Guid assignmentId)
        {
            var grades = await _unitOfWork.GradeRepository.GetByAssignmentIdAsync(assignmentId);
            var rounds = await _unitOfWork.GradeRoundRepository.GetByAssignmentIdAsync(assignmentId);

            var users = await _unitOfWork.UserRepository.GetAllAsync();
            var lecturers = users.Where(u => u.Role == "Lecturer").ToList();
            var submissions = await _unitOfWork.SubmissionRepository.GetSubmissionsByAssignmentIdAsync(assignmentId);

            if (!rounds.Any(gr => gr.RoundNumber == 1))
            {
                return await AutoAssignRound1Async(assignedByUserId, assignmentId, submissions!.ToList(), lecturers, users.ToList());
            }
            else if (!rounds.Any(gr => gr.RoundNumber == 2))
            {
                var gradesWithRegradeRequest = grades.Where(g => g.RegradeRequests != null && g.RegradeRequests.Any(rr => rr.Status == "Approved")).ToList();
                if (gradesWithRegradeRequest.Any())
                {
                    return await AutoAssignRound2Async(assignedByUserId, assignmentId, submissions!.ToList(), lecturers, users.ToList());
                }
            }
            else if (!rounds.Any(gr => gr.RoundNumber == 3))
            {
                var gradesWithRegradeRequest = grades.Where(g => g.RegradeRequests != null && g.RegradeRequests.Any(rr => rr.Status == "Approved")).ToList();
                if (gradesWithRegradeRequest.Any())
                {
                    return await AutoAssignRound3Async(assignedByUserId, assignmentId);
                }
            }

            return false;
        }

        private async Task SendNotificationEmailRound1Async(List<AssignmentDistribution> assignmentDistributions, List<User> lecturers, List<User> users)
        {
            var lecturerAssignments = assignmentDistributions.GroupBy(d => d.LecturerId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var lecturerId in lecturerAssignments.Keys)
            {
                var lecturer = lecturers.FirstOrDefault(l => l.Id == lecturerId);
                if (lecturer != null)
                {
                    var assignments = lecturerAssignments[lecturerId];
                    var listStudents = string.Join("<br/>", assignments.Select(a =>
                    {
                        var student = users.FirstOrDefault(u => u.Id == a.StudentId);
                        return $"- {student?.FullName} - {student?.Id}";
                    }));

                    var subject = "New Grading Assignments";
                    var body = $"Dear {lecturer.FullName},<br/>" +
                               $"You have been assigned to review the following students:<br/>{listStudents}<br/>" +
                               $"Please login to the system to start grading.";

                    await _emailService.SendMailAsync(lecturer.Email, subject, body);
                }

            }
        }

        private async Task SendNotificationEmailRound2Async(List<GradeRound> gradeRounds, List<User> lecturers, List<User> users)
        {
            var lecturerGroups = gradeRounds.GroupBy(gr => gr.LecturerId);

            foreach (var group in lecturerGroups)
            {
                var lecturer = lecturers.FirstOrDefault(l => l.Id == group.Key);
                if (lecturer == null) continue;

                var listStudents = string.Join("<br/>", group.Select(gr =>
                {
                    var student = users.FirstOrDefault(u => u.Id == gr.Grade?.StudentId);
                    return $"- {student?.FullName} ({student?.Id}), cùng với Co-Lecturer: {users.FirstOrDefault(u => u.Id == gr.CoLecturerId)?.FullName}";
                }));

                var subject = "Re-grading Assignments (Round 2)";
                var body = $"Dear {lecturer.FullName},<br/>" +
                           $"You have been assigned to regrade the following students:<br/>{listStudents}<br/>" +
                           $"Please login to the system to continue grading.";
                await _emailService.SendMailAsync(lecturer.Email, subject, body);
            }
        }

        private async Task<bool> AutoAssignRound1Async(string assignedBy, Guid assignmentId, List<Submission> submissions, List<User> lecturers, List<User> users)
        {
            var now = DateTime.Now;
            var newDistributions = new List<AssignmentDistribution>();
            var grades = await _unitOfWork.GradeRepository.GetAllAsync();

            for (int i = 0; i < submissions.Count; i++)
            {
                var submission = submissions[i];
                int j = i % lecturers.Count();
                var lecturer = lecturers[j];

                var assignmentDistribution = new AssignmentDistribution()
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = assignmentId,
                    AssignedBy = assignedBy,
                    LecturerId = lecturer.Id,
                    AssignedAt = now,
                    StudentId = submission.StudentId,
                    UpdatedAt = now,
                };

                newDistributions.Add(assignmentDistribution);

                var grade = grades.FirstOrDefault(g => g.AssignmentId == assignmentId && g.StudentId == submission.StudentId);
                if (grade == null)
                {
                    grade = new Grade
                    {
                        Id = Guid.NewGuid(),
                        AssignmentId = assignmentId,
                        StudentId = submission.StudentId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        ConfirmBy = ""
                    };
                    await _unitOfWork.GradeRepository.CreateAsync(grade);
                }

                var gradeRounds = new GradeRound
                {
                    Id = Guid.NewGuid(),
                    GradeId = grade.Id,
                    RoundNumber = 1,
                    LecturerId = lecturer.Id,
                    CoLecturerId = "",
                    Note = "",
                    MeetingUrl = "",
                };
                await _unitOfWork.GradeRoundRepository.CreateAsync(gradeRounds);
            }

            await _unitOfWork.DistributionRepository.AddRangeAsync(newDistributions);
            await _unitOfWork.SaveChangesAsync();

            await SendNotificationEmailRound1Async(newDistributions, lecturers, users);

            return true;
        }

        private async Task<bool> AutoAssignRound2Async(string assignedByUserId, Guid assignmentId, List<Submission> submissions, List<User> lecturers, List<User> users)
        {
            var now = DateTime.Now;
            var grades = await _unitOfWork.GradeRepository.GetByAssignmentIdAsync(assignmentId);
            var newGradeRounds = new List<GradeRound>();

            var gradesWithRegradeRequest = grades.Where(g => g.RegradeRequests != null && g.RegradeRequests.Any(rr => rr.Status == "Approved")).ToList();
            if (!gradesWithRegradeRequest.Any()) return false;

            foreach (var grade in gradesWithRegradeRequest)
            {
                var submission = submissions.FirstOrDefault(s => s.StudentId == grade.StudentId);
                if (submission == null) continue;

                var firstRound = await _unitOfWork.GradeRoundRepository.GetByGradeIdAndNumberAsync(grade.Id, 1);
                var availableLecturers = lecturers.Where(l => l.Id != firstRound?.LecturerId).ToList();
                if (!availableLecturers.Any())
                {
                    availableLecturers = lecturers;
                }

                var lecturer = availableLecturers[new Random().Next(availableLecturers.Count)];

                var gradeRound = new GradeRound
                {
                    Id = Guid.NewGuid(),
                    GradeId = grade.Id,
                    RoundNumber = 2,
                    LecturerId = firstRound?.LecturerId,
                    CoLecturerId = lecturer.Id,
                    Note = "",
                    MeetingUrl = ""
                };
                newGradeRounds.Add(gradeRound);
                await _unitOfWork.GradeRoundRepository.CreateAsync(gradeRound);
            }
            await _unitOfWork.SaveChangesAsync();

            await SendNotificationEmailRound2Async(newGradeRounds, lecturers, users);

            return true;
        }

        private async Task<bool> AutoAssignRound3Async(string assignedByUserId, Guid assignmentId)
        {
            var grades = await _unitOfWork.GradeRepository.GetAllAsync();
            var gradeRounds = await _unitOfWork.GradeRoundRepository.GetAllAsync();
            var users = await _unitOfWork.UserRepository.GetAllAsync();

            var gradesOfAssignment = grades.Where(g => g.AssignmentId == assignmentId).ToList();

            var now = DateTime.Now;
            var newGradeRounds = new List<GradeRound>();

            var gradesWithRegradeRequest = gradesOfAssignment.Where(g => g.RegradeRequests != null && g.RegradeRequests.Any(rr => rr.Status == "Approved")).ToList();
            if (!gradesWithRegradeRequest.Any()) return false;

            foreach (var grade in gradesWithRegradeRequest)
            {
                var roundsForStudent = gradeRounds.Where(gr => gr.GradeId == grade.Id).OrderBy(gr => gr.RoundNumber).ToList();
                if (roundsForStudent.Count < 2)
                {
                    continue;
                }

                var lecturer1 = users.FirstOrDefault(u => u.Id == roundsForStudent[0].LecturerId);
                var lecturer2 = users.FirstOrDefault(u => u.Id == roundsForStudent[1].LecturerId);
                var student = users.FirstOrDefault(u => u.Id == grade.StudentId);

                if (lecturer1 == null || lecturer2 == null || student == null)
                {
                    continue;
                }

                var roomName = $"Review-{assignmentId:N}-{Guid.NewGuid():N}";
                var meetingUrl = $"https://meet.jit.si/{roomName}";

                var gradeRound = new GradeRound
                {
                    Id = Guid.NewGuid(),
                    GradeId = grade.Id,
                    RoundNumber = 3,
                    LecturerId = lecturer1.Id,
                    CoLecturerId = lecturer2.Id,
                    ScheduleAt = FindNextAvailableSlot(lecturer1, lecturer2, gradeRounds.ToList(), now.AddDays(1).AddHours(8), 30),
                    MeetingUrl = meetingUrl,
                    Note = "",
                };

                newGradeRounds.Add(gradeRound);

                var subject = "Round 3 Meeting Scheduled";
                var body = $"Dear {lecturer1.FullName}, {lecturer2.FullName} and {student.FullName},<br/>" +
                           $"You are invited to join the round 3 meeting to review the submission.<br/>" +
                           $"Meeting link: <a href='{meetingUrl}'>{meetingUrl}</a><br/>" +
                           $"Scheduled at: {gradeRound.ScheduleAt}";

                var task = new List<Task>();
                task.Add(_emailService.SendMailAsync(lecturer1.Email, subject, body));
                task.Add(_emailService.SendMailAsync(lecturer2.Email, subject, body));
                task.Add(_emailService.SendMailAsync(student.Email, subject, body));
                await Task.WhenAll(task);
            }

            if (newGradeRounds.Any())
            {
                await _unitOfWork.GradeRoundRepository.AddRangeAsync(newGradeRounds);
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        private DateTime FindNextAvailableSlot(User lecturer1, User lecturer2, List<GradeRound> rounds, DateTime start, int bufferMinutes)
        {
            var time = start;
            while (true)
            {
                bool isLecturer1Busy = rounds.Any(gr => (gr.LecturerId == lecturer1.Id || gr.CoLecturerId == lecturer1.Id)
                                                && gr.ScheduleAt.HasValue
                                                && Math.Abs((gr.ScheduleAt.Value - time).TotalMinutes) < bufferMinutes);

                bool isLecturer2Busy = rounds.Any(gr => (gr.LecturerId == lecturer2.Id || gr.CoLecturerId == lecturer2.Id)
                                                && gr.ScheduleAt.HasValue
                                                && Math.Abs((gr.ScheduleAt.Value - time).TotalMinutes) < bufferMinutes);

                if (!isLecturer1Busy && !isLecturer2Busy)
                {
                    return time;
                }

                time = time.AddMinutes(30);
            }
        }
    }
}
