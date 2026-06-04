using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tenanzia.API.Models;

public partial class TenanziaContext : DbContext
{
    public TenanziaContext()
    {
    }

    public TenanziaContext(DbContextOptions<TenanziaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }
    public virtual DbSet<UserTenant> UserTenants { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<TaskItem> Tasks { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<Plan> Plans { get; set; }
    public virtual DbSet<Subscription> Subscriptions { get; set; }
    public virtual DbSet<TaskComment> TaskComments { get; set; }
    public virtual DbSet<TaskActivity> TaskActivities { get; set; }
    public virtual DbSet<TaskCommentRead> TaskCommentReads { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07970E7DDD");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07EF8C2512");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRole__3214EC07D1B219AD");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__UserRoles__RoleI__3C69FB99");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRoles__UserI__3B75D760");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
