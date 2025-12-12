using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SIMS.Data.Entities;

public partial class EventManagementDbContext : DbContext
{
    public EventManagementDbContext()
    {
    }

    public EventManagementDbContext(DbContextOptions<EventManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventBudget> EventBudgets { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Participation> Participations { get; set; }

    public virtual DbSet<Picture> Pictures { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<VEventFinancialSummary> VEventFinancialSummaries { get; set; }

    public virtual DbSet<VMemberParticipationSummary> VMemberParticipationSummaries { get; set; }

    public virtual DbSet<VUpcomingEvent> VUpcomingEvents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=EventManagementDB;Trusted_Connection=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK__Announce__9DE445544F7F8919");

            entity.ToTable("Announcement", "ems");

            entity.Property(e => e.AnnouncementId).HasColumnName("AnnouncementID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Announcement_Member");

            entity.HasOne(d => d.Event).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Announcement_Event");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69263C772805E1");

            entity.ToTable("Attendance", "ems");

            entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");
            entity.Property(e => e.CheckInTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ParticipationId).HasColumnName("ParticipationID");

            entity.HasOne(d => d.Participation).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ParticipationId)
                .HasConstraintName("FK_Attendance_Participation");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => new { e.LogId, e.TimeStamp });

            entity.ToTable("AuditLog", "ems");

            entity.Property(e => e.LogId)
                .ValueGeneratedOnAdd()
                .HasColumnName("LogID");
            entity.Property(e => e.TimeStamp).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.TableName).HasMaxLength(150);
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.BudgetId).HasName("PK__Budget__E38E79C492D4C67A");

            entity.ToTable("Budget", "ems");

            entity.Property(e => e.BudgetId).HasColumnName("BudgetID");
            entity.Property(e => e.BudgetName).HasMaxLength(150);
            entity.Property(e => e.FundsUsed).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RemainingFunds)
                .HasComputedColumnSql("([TotalFunds]-[FundsUsed])", true)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.TotalFunds).HasColumnType("decimal(14, 2)");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Event__7944C870D06DAFCD");

            entity.ToTable("Event", "ems", tb => tb.HasTrigger("tr_Event_InsteadOfDelete"));

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.BudgetAllocated).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.Venue).HasMaxLength(150);

            entity.HasOne(d => d.OrganizedByNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => d.OrganizedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Event_Organizer");
        });

        modelBuilder.Entity<EventBudget>(entity =>
        {
            entity.HasKey(e => e.EventBudgetId).HasName("PK__EventBud__77EB34143DC7EE1A");

            entity.ToTable("EventBudget", "ems");

            entity.HasIndex(e => e.BudgetId, "UQ_EventBudget_Budget").IsUnique();

            entity.Property(e => e.EventBudgetId).HasColumnName("EventBudgetID");
            entity.Property(e => e.AmountAllocated).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.BudgetId).HasColumnName("BudgetID");
            entity.Property(e => e.EventId).HasColumnName("EventID");

            entity.HasOne(d => d.Budget).WithOne(p => p.EventBudget)
                .HasForeignKey<EventBudget>(d => d.BudgetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventBudget_Budget");

            entity.HasOne(d => d.Event).WithMany(p => p.EventBudgets)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_EventBudget_Event");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => new { e.ExpenseId, e.ExpenseDate });

            entity.ToTable("Expense", "ems", tb =>
                {
                    tb.HasTrigger("tr_Expense_AfterInsert");
                    tb.HasTrigger("tr_Expense_AfterUpdate");
                });

            entity.HasIndex(e => new { e.BudgetId, e.ExpenseDate }, "IX_Expense_Budget_Date");

            entity.Property(e => e.ExpenseId)
                .ValueGeneratedOnAdd()
                .HasColumnName("ExpenseID");
            entity.Property(e => e.ExpenseDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Amount).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.BudgetId).HasColumnName("BudgetID");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Budget).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.BudgetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Expense_Budget");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Expense_Member");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Member__0CF04B3886AFEDBD");

            entity.ToTable("Member", "ems");

            entity.HasIndex(e => e.Email, "UQ__Member__A9D10534692FD914").IsUnique();

            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.JoinDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role).WithMany(p => p.Members)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Member_Role");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E329DEF4DE9");

            entity.ToTable("Notification", "ems");

            entity.HasIndex(e => e.MemberId, "IX_Notification_Unread").HasFilter("([IsRead]=(0))");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");

            entity.HasOne(d => d.Member).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK_Notification_Member");
        });

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.HasKey(e => e.ParticipationId).HasName("PK__Particip__4EA270802B4F4C4B");

            entity.ToTable("Participation", "ems");

            entity.HasIndex(e => new { e.EventId, e.Status }, "IX_Participation_Event_Status");

            entity.HasIndex(e => new { e.EventId, e.MemberId }, "UQ_Participation").IsUnique();

            entity.Property(e => e.ParticipationId).HasColumnName("ParticipationID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Event).WithMany(p => p.Participations)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Part_Event");

            entity.HasOne(d => d.Member).WithMany(p => p.Participations)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK_Part_Member");
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.HasKey(e => e.PictureId).HasName("PK__Pictures__8C2866F8F62A4DFE");

            entity.ToTable("Pictures", "ems");

            entity.Property(e => e.PictureId).HasColumnName("PictureID");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Event).WithMany(p => p.Pictures)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_Pictures_Event");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3A32FB0D6A");

            entity.ToTable("Role", "ems");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B6160C9F53381").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<VEventFinancialSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_EventFinancialSummary", "ems");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.EventTitle).HasMaxLength(150);
            entity.Property(e => e.OrganizedBy).HasMaxLength(150);
            entity.Property(e => e.RemainingAmount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalAllocated).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.TotalExpenses).HasColumnType("decimal(14, 2)");
        });

        modelBuilder.Entity<VMemberParticipationSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_MemberParticipationSummary", "ems");

            entity.Property(e => e.AttendanceRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<VUpcomingEvent>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_UpcomingEvents", "ems");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Organizer).HasMaxLength(150);
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.Venue).HasMaxLength(150);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
