using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Shared.Infrastructure.Persistence.Support;

public partial class SupportDbContext : DbContext
{
    public SupportDbContext(DbContextOptions<SupportDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessRole> AccessRoles { get; set; }

    public virtual DbSet<Case> Cases { get; set; }

    public virtual DbSet<CaseNote> CaseNotes { get; set; }

    public virtual DbSet<Complaint> Complaints { get; set; }

    public virtual DbSet<Dependent> Dependents { get; set; }

    public virtual DbSet<EmployerGroup> EmployerGroups { get; set; }

    public virtual DbSet<Escalation> Escalations { get; set; }

    public virtual DbSet<Interaction> Interactions { get; set; }

    public virtual DbSet<LoginAttempt> LoginAttempts { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK_iam_AccessRole");

            entity.ToTable("AccessRole", "iam");

            entity.HasIndex(e => e.RoleName, "UQ_iam_AccessRole_Name").IsUnique();

            entity.Property(e => e.RoleId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(400);
            entity.Property(e => e.RoleName)
                .HasMaxLength(80)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK_support_Case");

            entity.ToTable("Case", "support");

            entity.HasIndex(e => new { e.AssignedToUserId, e.Status }, "IX_support_Case_Assigned");

            entity.HasIndex(e => new { e.MemberId, e.Status }, "IX_support_Case_Member");

            entity.HasIndex(e => new { e.AssignedQueue, e.Status }, "IX_support_Case_Queue");

            entity.HasIndex(e => e.SlaDueUtc, "IX_support_Case_Sla").HasFilter("([Status]<>'Resolved' AND [Status]<>'Closed' AND [Status]<>'Cancelled')");

            entity.Property(e => e.CaseId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.AssignedQueue)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.AssignedToUserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.CaseType)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CorrelationId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBySource)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedByUserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.CreatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_support_Case_Created");
            entity.Property(e => e.GroupPolicyId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.MemberId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.Priority)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Normal", "DF_support_Case_Priority");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("New", "DF_support_Case_Status");
            entity.Property(e => e.Subject).HasMaxLength(300);
            entity.Property(e => e.UpdatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_support_Case_Updated");

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.Cases)
                .HasForeignKey(d => d.AssignedToUserId)
                .HasConstraintName("FK_support_Case_AssignedTo");

            entity.HasOne(d => d.GroupPolicy).WithMany(p => p.Cases)
                .HasForeignKey(d => d.GroupPolicyId)
                .HasConstraintName("FK_support_Case_Group");

            entity.HasOne(d => d.Member).WithMany(p => p.Cases)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK_support_Case_Member");
        });

        modelBuilder.Entity<CaseNote>(entity =>
        {
            entity.HasKey(e => e.CaseNoteId).HasName("PK_support_CaseNote");

            entity.ToTable("CaseNote", "support");

            entity.HasIndex(e => new { e.CaseId, e.CreatedUtc }, "IX_support_CaseNote_Case").IsDescending(false, true);

            entity.Property(e => e.AuthorType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AuthorUserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.CaseId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CreatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_support_CaseNote_Created");
            entity.Property(e => e.IsInternal).HasDefaultValue(true, "DF_support_CaseNote_Internal");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseNotes)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK_support_CaseNote_Case");
        });

        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId).HasName("PK_support_Complaint");

            entity.ToTable("Complaint", "support");

            entity.HasIndex(e => e.DueUtc, "IX_support_Complaint_Due").HasFilter("([Status]<>'Resolved' AND [Status]<>'Dismissed')");

            entity.HasIndex(e => new { e.MemberId, e.Status }, "IX_support_Complaint_Member");

            entity.Property(e => e.ComplaintId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CaseId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.ComplaintType)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.FiledUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_support_Complaint_Filed");
            entity.Property(e => e.MemberId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.RegulatorAgency)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Severity)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Filed", "DF_support_Complaint_Status");

            entity.HasOne(d => d.Case).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_support_Complaint_Case");

            entity.HasOne(d => d.Member).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_support_Complaint_Member");
        });

        modelBuilder.Entity<Dependent>(entity =>
        {
            entity.HasKey(e => e.DependentId).HasName("PK_member_Dependent");

            entity.ToTable("Dependent", "member");

            entity.Property(e => e.DependentId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_member_Dependent_IsActive");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MemberId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.Relationship)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Member).WithMany(p => p.Dependents)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_member_Dependent_Member");
        });

        modelBuilder.Entity<EmployerGroup>(entity =>
        {
            entity.HasKey(e => e.GroupPolicyId).HasName("PK_member_EmployerGroup");

            entity.ToTable("EmployerGroup", "member");

            entity.Property(e => e.GroupPolicyId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.CreatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_member_EmployerGroup_Created");
            entity.Property(e => e.GroupName).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active", "DF_member_EmployerGroup_Status");
            entity.Property(e => e.TpaId)
                .HasMaxLength(32)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Escalation>(entity =>
        {
            entity.HasKey(e => e.EscalationId).HasName("PK_support_Escalation");

            entity.ToTable("Escalation", "support");

            entity.HasIndex(e => e.CaseId, "IX_support_Escalation_Case");

            entity.HasIndex(e => new { e.ToQueue, e.Status }, "IX_support_Escalation_Queue");

            entity.Property(e => e.EscalationId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CaseId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.EscalatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_support_Escalation_Escalated");
            entity.Property(e => e.FromUserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Reason)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.ReasonDetail).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Open", "DF_support_Escalation_Status");
            entity.Property(e => e.ToQueue)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.ToUserId)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.Case).WithMany(p => p.Escalations)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK_support_Escalation_Case");

            entity.HasOne(d => d.FromUser).WithMany(p => p.EscalationFromUsers)
                .HasForeignKey(d => d.FromUserId)
                .HasConstraintName("FK_support_Escalation_From");

            entity.HasOne(d => d.ToUser).WithMany(p => p.EscalationToUsers)
                .HasForeignKey(d => d.ToUserId)
                .HasConstraintName("FK_support_Escalation_To");
        });

        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.HasKey(e => e.InteractionId).HasName("PK_support_Interaction");

            entity.ToTable("Interaction", "support");

            entity.HasIndex(e => e.CaseId, "IX_support_Interaction_Case");

            entity.HasIndex(e => new { e.MemberId, e.StartedUtc }, "IX_support_Interaction_Member").IsDescending(false, true);

            entity.HasIndex(e => e.SessionId, "IX_support_Interaction_Session");

            entity.Property(e => e.InteractionId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CaseId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Direction)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.HandledByUserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Intent)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.MemberId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.SentimentScore).HasColumnType("decimal(4, 3)");
            entity.Property(e => e.SessionId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.StartedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_support_Interaction_Started");
            entity.Property(e => e.TranscriptUri).HasMaxLength(500);

            entity.HasOne(d => d.Case).WithMany(p => p.Interactions)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK_support_Interaction_Case");

            entity.HasOne(d => d.HandledByUser).WithMany(p => p.Interactions)
                .HasForeignKey(d => d.HandledByUserId)
                .HasConstraintName("FK_support_Interaction_HandledBy");

            entity.HasOne(d => d.Member).WithMany(p => p.Interactions)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK_support_Interaction_Member");
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("PK_iam_LoginAttempt");

            entity.ToTable("LoginAttempt", "iam");

            entity.HasIndex(e => new { e.UserId, e.AttemptUtc }, "IX_iam_LoginAttempt_User").IsDescending(false, true);

            entity.Property(e => e.AttemptUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_iam_LoginAttempt_Utc");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.FailureReason)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.UserAgent).HasMaxLength(400);
            entity.Property(e => e.UserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.LoginAttempts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_iam_LoginAttempt_User");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK_member_Member");

            entity.ToTable("Member", "member");

            entity.HasIndex(e => e.GroupPolicyId, "IX_member_Member_Group");

            entity.Property(e => e.MemberId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.AddressLine1).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_member_Member_Created");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.EmploymentStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active", "DF_member_Member_EmpStatus");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.GroupPolicyId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SalaryBand)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SsnLast4)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UpdatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_member_Member_Updated");
            entity.Property(e => e.Zip)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.GroupPolicy).WithMany(p => p.Members)
                .HasForeignKey(d => d.GroupPolicyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_member_Member_Group");
        });

        modelBuilder.Entity<PasswordResetRequest>(entity =>
        {
            entity.HasKey(e => e.ResetRequestId).HasName("PK_iam_PasswordResetRequest");

            entity.ToTable("PasswordResetRequest", "iam");

            entity.HasIndex(e => new { e.UserId, e.Status }, "IX_iam_PasswordReset_User");

            entity.Property(e => e.ResetRequestId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Channel)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.InitiatedBy)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.RequestedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_iam_PasswordReset_Requested");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending", "DF_iam_PasswordReset_Status");
            entity.Property(e => e.UserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.VerificationMethod)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_iam_PasswordReset_User");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_iam_UserAccount");

            entity.ToTable("UserAccount", "iam");

            entity.HasIndex(e => e.Email, "UQ_iam_UserAccount_Email").IsUnique();

            entity.HasIndex(e => e.OktaUserId, "UQ_iam_UserAccount_OktaId").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_iam_UserAccount_Username").IsUnique();

            entity.Property(e => e.UserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.CreatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_iam_UserAccount_Created");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.MemberId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.OktaUserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active", "DF_iam_UserAccount_Status");
            entity.Property(e => e.UserType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasOne(d => d.Member).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK_iam_UserAccount_Member");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK_iam_UserRole");

            entity.ToTable("UserRole", "iam");

            entity.Property(e => e.UserId)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.RoleId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.GrantedBy)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.GrantedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_iam_UserRole_Granted");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_iam_UserRole_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_iam_UserRole_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
