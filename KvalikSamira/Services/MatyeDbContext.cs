using Microsoft.EntityFrameworkCore;
using KvalikSamira.Models;

namespace KvalikSamira.Services
{
    public class MatyeDbContext : DbContext
    {
        public DbSet<Role> roles { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Collection> collections { get; set; }
        public DbSet<ServiceType> serviceTypes { get; set; }
        public DbSet<Service> services { get; set; }
        public DbSet<MasterService> masterServices { get; set; }
        public DbSet<Qualification> qualifications { get; set; }
        public DbSet<Appointment> appointments { get; set; }
        public DbSet<Review> reviews { get; set; }
        public DbSet<QualificationRequest> qualificationRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=matye;Username=postgres;Password=123");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.id);
                entity.Property(e => e.name).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Collection>(entity =>
            {
                entity.ToTable("collections");
                entity.HasKey(e => e.id);
                entity.Property(e => e.name).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<ServiceType>(entity =>
            {
                entity.ToTable("service_types");
                entity.HasKey(e => e.id);
                entity.Property(e => e.name).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.id);
                entity.Property(e => e.lastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.firstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.patronymic).HasMaxLength(100);
                entity.Property(e => e.login).HasMaxLength(100).IsRequired();
                entity.Property(e => e.password).HasMaxLength(255).IsRequired();
                entity.Property(e => e.balance).HasColumnType("decimal(10,2)");
                entity.Property(e => e.roleId).HasColumnName("role_id");
                entity.HasOne(e => e.role).WithMany(r => r.users).HasForeignKey(e => e.roleId);
            });

            modelBuilder.Entity<Qualification>(entity =>
            {
                entity.ToTable("qualifications");
                entity.HasKey(e => e.id);
                entity.Property(e => e.userId).HasColumnName("user_id");
                entity.Property(e => e.level).HasMaxLength(50);
                entity.HasOne(e => e.user).WithOne(u => u.qualification).HasForeignKey<Qualification>(e => e.userId);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("services");
                entity.HasKey(e => e.id);
                entity.Property(e => e.name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.price).HasColumnType("decimal(10,2)");
                entity.Property(e => e.durationMinutes).HasColumnName("duration_minutes");
                entity.Property(e => e.imagePath).HasColumnName("image_path").HasMaxLength(255);
                entity.Property(e => e.serviceTypeId).HasColumnName("service_type_id");
                entity.Property(e => e.collectionId).HasColumnName("collection_id");
                entity.Property(e => e.createdAt).HasColumnName("created_at");
                entity.Property(e => e.modifiedAt).HasColumnName("modified_at");
                entity.HasOne(e => e.serviceType).WithMany(st => st.services).HasForeignKey(e => e.serviceTypeId);
                entity.HasOne(e => e.collection).WithMany(c => c.services).HasForeignKey(e => e.collectionId);
            });

            modelBuilder.Entity<MasterService>(entity =>
            {
                entity.ToTable("master_services");
                entity.HasKey(e => e.id);
                entity.Property(e => e.userId).HasColumnName("user_id");
                entity.Property(e => e.serviceId).HasColumnName("service_id");
                entity.HasOne(e => e.user).WithMany(u => u.masterServices).HasForeignKey(e => e.userId);
                entity.HasOne(e => e.service).WithMany(s => s.masterServices).HasForeignKey(e => e.serviceId);
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("appointments");
                entity.HasKey(e => e.id);
                entity.Property(e => e.userId).HasColumnName("user_id");
                entity.Property(e => e.serviceId).HasColumnName("service_id");
                entity.Property(e => e.masterId).HasColumnName("master_id");
                entity.Property(e => e.queueNumber).HasColumnName("queue_number");
                entity.Property(e => e.appointmentDate).HasColumnName("appointment_date");
                entity.Property(e => e.createdAt).HasColumnName("created_at");
                entity.HasOne(e => e.user).WithMany(u => u.appointments).HasForeignKey(e => e.userId);
                entity.HasOne(e => e.service).WithMany(s => s.appointments).HasForeignKey(e => e.serviceId);
                entity.HasOne(e => e.master).WithMany().HasForeignKey(e => e.masterId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("reviews");
                entity.HasKey(e => e.id);
                entity.Property(e => e.userId).HasColumnName("user_id");
                entity.Property(e => e.serviceId).HasColumnName("service_id");
                entity.Property(e => e.masterId).HasColumnName("master_id");
                entity.Property(e => e.text).IsRequired();
                entity.Property(e => e.createdAt).HasColumnName("created_at");
                entity.HasOne(e => e.user).WithMany(u => u.reviews).HasForeignKey(e => e.userId);
                entity.HasOne(e => e.service).WithMany(s => s.reviews).HasForeignKey(e => e.serviceId);
                entity.HasOne(e => e.master).WithMany().HasForeignKey(e => e.masterId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<QualificationRequest>(entity =>
            {
                entity.ToTable("qualification_requests");
                entity.HasKey(e => e.id);
                entity.Property(e => e.userId).HasColumnName("user_id");
                entity.Property(e => e.currentLevel).HasColumnName("current_level").HasMaxLength(50);
                entity.Property(e => e.requestedLevel).HasColumnName("requested_level").HasMaxLength(50);
                entity.Property(e => e.status).HasMaxLength(50);
                entity.Property(e => e.createdAt).HasColumnName("created_at");
                entity.HasOne(e => e.user).WithMany(u => u.qualificationRequests).HasForeignKey(e => e.userId);
            });
        }
    }
}
