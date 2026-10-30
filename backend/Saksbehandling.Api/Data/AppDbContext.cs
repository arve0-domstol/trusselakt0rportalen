using Microsoft.EntityFrameworkCore;
using Saksbehandling.Api.Models;

namespace Saksbehandling.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseAssignment> CaseAssignments => Set<CaseAssignment>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(e =>
        {
            e.HasIndex(d => d.Code).IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(c => c.SyntheticNationalId).IsUnique();
        });

        modelBuilder.Entity<Case>(e =>
        {
            e.HasIndex(c => c.CaseNumber).IsUnique();

            e.HasOne(c => c.Customer)
                .WithMany(cu => cu.Cases)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Department)
                .WithMany(d => d.Cases)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.AssignedToUser)
                .WithMany(u => u.AssignedCases)
                .HasForeignKey(c => c.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(c => c.ApprovedByUser)
                .WithMany()
                .HasForeignKey(c => c.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            e.Property(c => c.Deadline)
                .HasConversion(
                    v => v == null ? null : (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)),
                    v => v == null ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                );
        });

        modelBuilder.Entity<CaseAssignment>(e =>
        {
            e.HasOne(a => a.Case)
                .WithMany(c => c.AssignmentHistory)
                .HasForeignKey(a => a.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.AssignedToUser)
                .WithMany()
                .HasForeignKey(a => a.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.AssignedByUser)
                .WithMany()
                .HasForeignKey(a => a.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(e =>
        {
            e.HasOne(c => c.Case)
                .WithMany(k => k.Comments)
                .HasForeignKey(c => c.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.AuthorUser)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Document>(e =>
        {
            e.HasOne(d => d.Case)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.UploadedByUser)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(d => d.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditEvent>(e =>
        {
            e.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(a => new { a.EntityType, a.EntityId });
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.RelatedCase)
                .WithMany(c => c.Notifications)
                .HasForeignKey(n => n.RelatedCaseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
