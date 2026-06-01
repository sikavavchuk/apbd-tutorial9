using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UniversityTasksDbFirstApi.Models;

namespace UniversityTasksDbFirstApi.Data;

public partial class UniversityTasksDbContext : DbContext
{
    public UniversityTasksDbContext(DbContextOptions<UniversityTasksDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Assignme__32499E770FBE921A");

            entity.HasIndex(e => e.CourseId, "IX_Assignments_CourseId");

            entity.HasIndex(e => e.DueDate, "IX_Assignments_DueDate");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(160);

            entity.HasOne(d => d.Course).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Assignmen__Cours__59063A47");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A7C411E1AA");

            entity.HasIndex(e => e.Code, "UQ__Courses__A25C5AA7274FC890").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(160);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771BA6377929");

            entity.HasIndex(e => e.CourseId, "IX_Enrollments_CourseId");

            entity.HasIndex(e => new { e.StudentId, e.CourseId }, "UQ__Enrollme__5E57FC828998713F").IsUnique();

            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Cours__5535A963");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Stude__5441852A");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B999DE6B80E");

            entity.HasIndex(e => e.IndexNumber, "UQ__Students__98DAB2EA4F94DE23").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Students__A9D105340B560874").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(160);
            entity.Property(e => e.FirstName).HasMaxLength(80);
            entity.Property(e => e.IndexNumber).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(80);
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__Submissi__449EE1259BC49F20");

            entity.HasIndex(e => e.AssignmentId, "IX_Submissions_AssignmentId");

            entity.HasIndex(e => e.Status, "IX_Submissions_Status");

            entity.HasIndex(e => e.StudentId, "IX_Submissions_StudentId");

            entity.HasIndex(e => new { e.AssignmentId, e.StudentId }, "UQ__Submissi__B165CCCFD18BC8BF").IsUnique();

            entity.Property(e => e.Feedback).HasMaxLength(1000);
            entity.Property(e => e.RepositoryUrl).HasMaxLength(300);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.Assignment).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__Assig__5EBF139D");

            entity.HasOne(d => d.Student).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__Stude__5FB337D6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
