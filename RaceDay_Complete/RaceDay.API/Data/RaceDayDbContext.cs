using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(e => {
            e.ToTable("Users"); e.HasKey(x => x.UserId); e.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(50).IsRequired(); e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Email).IsUnique(); e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired(); e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.Property(x => x.DateRegistered).HasDefaultValueSql("GETDATE()");
        });
        modelBuilder.Entity<Category>(e => {
            e.ToTable("Categories"); e.HasKey(x => x.CategoryId); e.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.CategoryName).IsUnique(); e.Property(x => x.Description).HasMaxLength(255);
        });
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, CategoryName = "Road Running", Description = "Running events held on paved roads" },
            new Category { CategoryId = 2, CategoryName = "Cycling", Description = "Road and trail cycling events" },
            new Category { CategoryId = 3, CategoryName = "Walking", Description = "Community walks and charity walk events" },
            new Category { CategoryId = 4, CategoryName = "Trail Running", Description = "Off-road running events on natural terrain" },
            new Category { CategoryId = 5, CategoryName = "Triathlon", Description = "Multi-discipline events combining swim, bike, and run" });
        modelBuilder.Entity<Event>(e => {
            e.ToTable("Events"); e.HasKey(x => x.EventId); e.Property(x => x.EventName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Location).HasMaxLength(150).IsRequired(); e.Property(x => x.RouteDescription).HasMaxLength(500);
            e.Property(x => x.Distance).HasMaxLength(50).IsRequired(); e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue(EventStatuses.Upcoming);
            e.HasOne(x => x.Organizer).WithMany(x => x.OrganisedEvents).HasForeignKey(x => x.OrganizerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(x => x.Events).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<EventCategory>(e => {
            e.ToTable("Event_Categories"); e.HasKey(x => x.EventCategoryId);
            e.HasIndex(x => new { x.EventId, x.CategoryId }).IsUnique();
            e.HasOne(x => x.Event).WithMany(x => x.EventCategories).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany(x => x.EventCategories).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Enrolment>(e => {
            e.ToTable("Enrolments"); e.HasKey(x => x.EnrolmentId); e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue(EnrolmentStatuses.Confirmed);
            e.HasIndex(x => new { x.UserId, x.EventId }).IsUnique();
            e.HasOne(x => x.User).WithMany(x => x.Enrolments).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Event).WithMany(x => x.Enrolments).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Result>(e => {
            e.ToTable("Results"); e.HasKey(x => x.ResultId); e.Property(x => x.FinishTime).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500); e.HasIndex(x => x.EnrolmentId).IsUnique();
            e.HasOne(x => x.Enrolment).WithOne(x => x.Result).HasForeignKey<Result>(x => x.EnrolmentId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
