using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pierre.Web.Domain.Entities;
using Pierre.Web.Domain.Enums;

namespace Pierre.Web.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<ConsultationNote> ConsultationNotes => Set<ConsultationNote>();
    public DbSet<ContentPost> ContentPosts => Set<ContentPost>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<ImportedFile> ImportedFiles => Set<ImportedFile>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("public");

        ConfigureIdentityTables(builder);
        ConfigureClient(builder);
        ConfigureAppointment(builder);
        ConfigureAvailability(builder);
        ConfigureConsultationNote(builder);
        ConfigureContentPost(builder);
        ConfigureContactRequest(builder);
        ConfigureInvoice(builder);
        ConfigureImportedFile(builder);
        ConfigureNotificationLog(builder);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<IdentityRole<Guid>>(entity => entity.ToTable("roles"));
        builder.Entity<IdentityUserRole<Guid>>(entity => entity.ToTable("user_roles"));
        builder.Entity<IdentityUserClaim<Guid>>(entity => entity.ToTable("user_claims"));
        builder.Entity<IdentityUserLogin<Guid>>(entity => entity.ToTable("user_logins"));
        builder.Entity<IdentityUserToken<Guid>>(entity => entity.ToTable("user_tokens"));
        builder.Entity<IdentityRoleClaim<Guid>>(entity => entity.ToTable("role_claims"));
    }

    private static void ConfigureClient(ModelBuilder builder)
    {
        builder.Entity<Client>(entity =>
        {
            entity.ToTable("clients");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(256);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.IsArchived).HasColumnName("is_archived");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.LastName).HasDatabaseName("ix_clients_last_name");
        });
    }

    private static void ConfigureAppointment(ModelBuilder builder)
    {
        builder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.SlotId).HasColumnName("slot_id").IsRequired();
            entity.Property(e => e.RequesterName).HasColumnName("requester_name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.RequesterEmail).HasColumnName("requester_email").HasMaxLength(256);
            entity.Property(e => e.RequesterPhone).HasColumnName("requester_phone").HasMaxLength(20);
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Status).HasDatabaseName("ix_appointments_status");

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Appointments)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Slot)
                .WithOne(a => a.Appointment)
                .HasForeignKey<Appointment>(e => e.SlotId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAvailability(ModelBuilder builder)
    {
        builder.Entity<Availability>(entity =>
        {
            entity.ToTable("availabilities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date").IsRequired();
            entity.Property(e => e.StartTime).HasColumnName("start_time").IsRequired();
            entity.Property(e => e.EndTime).HasColumnName("end_time").IsRequired();
            entity.Property(e => e.IsBlocked).HasColumnName("is_blocked");
        });
    }

    private static void ConfigureConsultationNote(ModelBuilder builder)
    {
        builder.Entity<ConsultationNote>(entity =>
        {
            entity.ToTable("consultation_notes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id").IsRequired();
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.Date).HasColumnName("date").IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.Recommendations).HasColumnName("recommendations");
            entity.Property(e => e.Weight).HasColumnName("weight").HasColumnType("decimal(5,2)");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Client)
                .WithMany(c => c.ConsultationNotes)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.ConsultationNote)
                .HasForeignKey<ConsultationNote>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureContentPost(ModelBuilder builder)
    {
        builder.Entity<ContentPost>(entity =>
        {
            entity.ToTable("content_posts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).HasColumnName("slug").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).HasColumnName("type").IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Body).HasColumnName("body").IsRequired();
            entity.Property(e => e.ImagePath).HasColumnName("image_path").HasMaxLength(500);
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Slug).IsUnique().HasDatabaseName("ix_content_posts_slug");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_content_posts_status");
        });
    }

    private static void ConfigureContactRequest(ModelBuilder builder)
    {
        builder.Entity<ContactRequest>(entity =>
        {
            entity.ToTable("contact_requests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(256);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
        });
    }

    private static void ConfigureInvoice(ModelBuilder builder)
    {
        builder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id").IsRequired();
            entity.Property(e => e.Reference).HasColumnName("reference").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnName("amount").IsRequired().HasColumnType("decimal(10,2)");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.IssuedAt).HasColumnName("issued_at").IsRequired();
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureImportedFile(ModelBuilder builder)
    {
        builder.Entity<ImportedFile>(entity =>
        {
            entity.ToTable("imported_files");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FileName).HasColumnName("file_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.ImportedAt).HasColumnName("imported_at");
            entity.Property(e => e.RowCount).HasColumnName("row_count");
            entity.Property(e => e.ErrorCount).HasColumnName("error_count");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasConversion<string>().HasMaxLength(20);
        });
    }

    private static void ConfigureNotificationLog(ModelBuilder builder)
    {
        builder.Entity<NotificationLog>(entity =>
        {
            entity.ToTable("notification_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RecipientEmail).HasColumnName("recipient_email").IsRequired().HasMaxLength(256);
            entity.Property(e => e.Subject).HasColumnName("subject").IsRequired().HasMaxLength(200);
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.Success).HasColumnName("success");
            entity.Property(e => e.Error).HasColumnName("error");
        });
    }
}
