using Microsoft.EntityFrameworkCore;
using SystemChecker.API.Models;

namespace SystemChecker.API.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Machine> Machines { get; set; }
    public DbSet<SystemCheckHistory> SystemCheckHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired();
            entity.HasIndex(e => e.Key).IsUnique();
            
            entity.HasOne(e => e.Machine)
                  .WithOne(m => m.ApiKey)
                  .HasForeignKey<ApiKey>(a => a.MachineId);
        });

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<SystemCheckHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            
            entity.HasOne(e => e.Machine)
                  .WithMany()
                  .HasForeignKey(e => e.MachineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            
            entity.HasOne(e => e.SystemCheckHistory)
                  .WithMany(h => h.Services)
                  .HasForeignKey(e => e.SystemCheckHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NetworkStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.SystemCheckHistory)
                  .WithOne(h => h.Network)
                  .HasForeignKey<NetworkStatus>(e => e.SystemCheckHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiskStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            
            entity.HasOne(e => e.SystemCheckHistory)
                  .WithMany(h => h.Disks)
                  .HasForeignKey(e => e.SystemCheckHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CpuStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.SystemCheckHistory)
                  .WithOne(h => h.Cpu)
                  .HasForeignKey<CpuStatus>(e => e.SystemCheckHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MemoryStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.SystemCheckHistory)
                  .WithOne(h => h.Memory)
                  .HasForeignKey<MemoryStatus>(e => e.SystemCheckHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TcpPortStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.SystemCheckHistory)
                  .WithMany(h => h.Ports)
                  .HasForeignKey(e => e.SystemCheckHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
} 