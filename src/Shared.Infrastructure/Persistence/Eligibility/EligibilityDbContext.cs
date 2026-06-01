using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Shared.Infrastructure.Persistence.Eligibility;

public partial class EligibilityDbContext : DbContext
{
    public EligibilityDbContext(DbContextOptions<EligibilityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Accumulator> Accumulators { get; set; }

    public virtual DbSet<BenefitPlan> BenefitPlans { get; set; }

    public virtual DbSet<CoverageHistory> CoverageHistories { get; set; }

    public virtual DbSet<Dependent> Dependents { get; set; }

    public virtual DbSet<EmployerGroup> EmployerGroups { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MemberCoverage> MemberCoverages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accumulator>(entity =>
        {
            entity.HasKey(e => e.AccumulatorId).HasName("PK_elig_Accumulator");

            entity.ToTable("Accumulator", "elig");

            entity.HasIndex(e => new { e.MemberId, e.PlanYear }, "IX_elig_Accumulator_Member");

            entity.HasIndex(e => new { e.MemberId, e.PlanId, e.PlanYear, e.AccumulatorType }, "UQ_elig_Accumulator_MemberPlanYearType").IsUnique();

            entity.Property(e => e.AccumulatorType)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.AppliedAmount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.LastUpdatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_elig_Accumulator_Updated");
            entity.Property(e => e.LimitAmount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.MemberId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.PlanId)
                .HasMaxLength(40)
                .IsUnicode(false);

            entity.HasOne(d => d.Member).WithMany(p => p.Accumulators)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_elig_Accumulator_Member");

            entity.HasOne(d => d.Plan).WithMany(p => p.Accumulators)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_elig_Accumulator_Plan");
        });

        modelBuilder.Entity<BenefitPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK_elig_BenefitPlan");

            entity.ToTable("BenefitPlan", "elig");

            entity.HasIndex(e => new { e.PlanName, e.PlanYear }, "UQ_elig_BenefitPlan_NameYear").IsUnique();

            entity.Property(e => e.PlanId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CarrierId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Coinsurance).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Copay).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DeductibleFam).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DeductibleInd).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LineOfBusiness)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NetworkType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.OopMaxFam).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OopMaxInd).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PlanName).HasMaxLength(200);
        });

        modelBuilder.Entity<CoverageHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK_elig_CoverageHistory");

            entity.ToTable("CoverageHistory", "elig");

            entity.HasIndex(e => e.CoverageId, "IX_elig_CoverageHistory_Coverage");

            entity.Property(e => e.ChangeType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ChangedBy)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.ChangedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_elig_CoverageHistory_Changed");
            entity.Property(e => e.CoverageId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.MemberId)
                .HasMaxLength(32)
                .IsUnicode(false);

            entity.HasOne(d => d.Member).WithMany(p => p.CoverageHistories)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_elig_CoverageHistory_Member");
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

        modelBuilder.Entity<MemberCoverage>(entity =>
        {
            entity.HasKey(e => e.CoverageId).HasName("PK_elig_MemberCoverage");

            entity.ToTable("MemberCoverage", "elig");

            entity.HasIndex(e => new { e.MemberId, e.Status }, "IX_elig_MemberCoverage_Member");

            entity.Property(e => e.CoverageId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.CreatedUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_elig_MemberCoverage_Created");
            entity.Property(e => e.MemberId)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.PlanId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.PrimaryCareProvNpi)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active", "DF_elig_MemberCoverage_Status");
            entity.Property(e => e.Tier)
                .HasMaxLength(40)
                .IsUnicode(false);

            entity.HasOne(d => d.Member).WithMany(p => p.MemberCoverages)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_elig_MemberCoverage_Member");

            entity.HasOne(d => d.Plan).WithMany(p => p.MemberCoverages)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_elig_MemberCoverage_Plan");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
