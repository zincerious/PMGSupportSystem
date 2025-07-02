using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using PMGSupport.ThangTQ.Repositories.Models;

namespace PMGSupport.ThangTQ.Repositories.DBContext;

public partial class SWD392Context : DbContext
{
    public SWD392Context()
    {
    }

    public SWD392Context(DbContextOptions<SWD392Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<AssignmentDistribution> AssignmentDistributions { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<GradeRound> GradeRounds { get; set; }

    public virtual DbSet<RegradeRequest> RegradeRequests { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public static string GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = config.GetConnectionString(connectionStringName)!;
        return connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assignme__3213E83FDD6E0E84");

            entity.ToTable("Assignment");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BaremPath)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("baremPath");
            entity.Property(e => e.ExaminerId)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("examinerId");
            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("filePath");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UploadedAt)
                .HasColumnType("datetime")
                .HasColumnName("uploadedAt");

            entity.HasOne(d => d.Examiner).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.ExaminerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Assignmen__exami__3B75D760");
        });

        modelBuilder.Entity<AssignmentDistribution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assignme__3213E83FEC131ACA");

            entity.ToTable("AssignmentDistribution");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AssignedAt)
                .HasColumnType("datetime")
                .HasColumnName("assignedAt");
            entity.Property(e => e.AssignedBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("assignedBy");
            entity.Property(e => e.AssignmentId).HasColumnName("assignmentId");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("lecturerId");
            entity.Property(e => e.StudentId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("studentId");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.AssignmentDistributionAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK__Assignmen__assig__52593CB8");

            entity.HasOne(d => d.Assignment).WithMany(p => p.AssignmentDistributions)
                .HasForeignKey(d => d.AssignmentId)
                .HasConstraintName("FK__Assignmen__assig__4F7CD00D");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.AssignmentDistributionLecturers)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__Assignmen__lectu__5165187F");

            entity.HasOne(d => d.Student).WithMany(p => p.AssignmentDistributionStudents)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Assignmen__stude__5070F446");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Grade__3213E83FCC35202F");

            entity.ToTable("Grade");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AssignmentId).HasColumnName("assignmentId");
            entity.Property(e => e.BiasPercent).HasColumnName("biasPercent");
            entity.Property(e => e.ConfirmBy)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("confirmBy");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FinalScore).HasColumnName("finalScore");
            entity.Property(e => e.StudentId)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("studentId");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Assignment).WithMany(p => p.Grades)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grade__assignmen__4222D4EF");

            entity.HasOne(d => d.ConfirmByNavigation).WithMany(p => p.GradeConfirmByNavigations)
                .HasForeignKey(d => d.ConfirmBy)
                .HasConstraintName("FK__Grade__confirmBy__440B1D61");

            entity.HasOne(d => d.Student).WithMany(p => p.GradeStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grade__studentId__4316F928");
        });

        modelBuilder.Entity<GradeRound>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GradeRou__3213E83FF75B1AB7");

            entity.ToTable("GradeRound");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CoLecturerId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("coLecturerId");
            entity.Property(e => e.GradeAt)
                .HasColumnType("datetime")
                .HasColumnName("gradeAt");
            entity.Property(e => e.GradeId).HasColumnName("gradeId");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("lecturerId");
            entity.Property(e => e.MeetingUrl)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("meetingUrl");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("note");
            entity.Property(e => e.RoundNumber).HasColumnName("roundNumber");
            entity.Property(e => e.ScheduleAt)
                .HasColumnType("datetime")
                .HasColumnName("scheduleAt");
            entity.Property(e => e.Score).HasColumnName("score");

            entity.HasOne(d => d.CoLecturer).WithMany(p => p.GradeRoundCoLecturers)
                .HasForeignKey(d => d.CoLecturerId)
                .HasConstraintName("FK__GradeRoun__coLec__48CFD27E");

            entity.HasOne(d => d.Grade).WithMany(p => p.GradeRounds)
                .HasForeignKey(d => d.GradeId)
                .HasConstraintName("FK__GradeRoun__grade__46E78A0C");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.GradeRoundLecturers)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__GradeRoun__lectu__47DBAE45");
        });

        modelBuilder.Entity<RegradeRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RegradeR__3213E83F8F054C75");

            entity.ToTable("RegradeRequest");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.GradeId).HasColumnName("gradeId");
            entity.Property(e => e.RequestAt)
                .HasColumnType("datetime")
                .HasColumnName("requestAt");
            entity.Property(e => e.RequestRound).HasColumnName("requestRound");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.StudentId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("studentId");

            entity.HasOne(d => d.Grade).WithMany(p => p.RegradeRequests)
                .HasForeignKey(d => d.GradeId)
                .HasConstraintName("FK__RegradeRe__grade__4BAC3F29");

            entity.HasOne(d => d.Student).WithMany(p => p.RegradeRequests)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__RegradeRe__stude__4CA06362");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Submissi__3213E83F0DF738DB");

            entity.ToTable("Submission");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AssignmentId).HasColumnName("assignmentId");
            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("filePath");
            entity.Property(e => e.StudentId)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("studentId");
            entity.Property(e => e.SubmittedAt)
                .HasColumnType("datetime")
                .HasColumnName("submittedAt");

            entity.HasOne(d => d.Assignment).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__assig__3E52440B");

            entity.HasOne(d => d.Student).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__stude__3F466844");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3213E83F0A83770D");

            entity.ToTable("User");

            entity.HasIndex(e => e.GoogleId);

            entity.HasIndex(e => e.Email, "UQ__User__AB6E61642408AB06").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("fullName");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("googleId");
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.StudentCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("studentCode");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}