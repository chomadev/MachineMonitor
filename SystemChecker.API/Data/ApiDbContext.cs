using Microsoft.EntityFrameworkCore;
using SystemChecker.API.Models;
using System.Text.Json;

namespace SystemChecker.API.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Machine> Machines { get; set; }
    public DbSet<SystemCheckHistory> SystemCheckHistory { get; set; }
    public DbSet<FolderStatus> FolderStatuses { get; set; }
    public DbSet<FolderChange> FolderChanges { get; set; }
    public DbSet<MonitoredAddress> MonitoredAddresses { get; set; }
    public DbSet<NetworkStatus> NetworkStatuses { get; set; }
    public DbSet<MachineConfiguration> MachineConfigurations { get; set; }
    public DbSet<FolderMonitorConfig> FolderMonitorConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FolderStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Path).IsRequired();
            
            entity.HasOne(e => e.SystemCheckHistory)
                  .WithMany(h => h.Folders)
                  .HasForeignKey(e => e.SystemCheckHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FolderChange>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Path).IsRequired();
            
            entity.HasOne(e => e.SystemCheckHistory)
                  .WithMany(h => h.FolderChanges)
                  .HasForeignKey(e => e.SystemCheckHistoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MonitoredAddress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).IsRequired();
            
            entity.HasOne(e => e.NetworkStatus)
                  .WithMany(h => h.MonitoredAddresses)
                  .HasForeignKey(e => e.NetworkStatusId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CpuStatus>(entity =>
        {
            entity.HasKey("Id");
            entity.Property<int>("Id").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<MemoryStatus>(entity =>
        {
            entity.HasKey("Id");
            entity.Property<int>("Id").ValueGeneratedOnAdd();
        });

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
            entity.HasOne(e => e.ApiKey)
                .WithOne(k => k.Machine)
                .HasForeignKey<ApiKey>(k => k.MachineId);
        });

        modelBuilder.Entity<SystemCheckHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            
            entity.HasOne(e => e.Machine)
                  .WithMany()
                  .HasForeignKey(e => e.MachineId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Services)
                .WithOne(s => s.SystemCheckHistory)
                .HasForeignKey(s => s.SystemCheckHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Network)
                .WithOne(n => n.SystemCheckHistory)
                .HasForeignKey<NetworkStatus>(n => n.SystemCheckHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Disks)
                .WithOne(d => d.SystemCheckHistory)
                .HasForeignKey(d => d.SystemCheckHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cpu)
                .WithOne(c => c.SystemCheckHistory)
                .HasForeignKey<CpuStatus>(c => c.SystemCheckHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Memory)
                .WithOne(m => m.SystemCheckHistory)
                .HasForeignKey<MemoryStatus>(m => m.SystemCheckHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Ports)
                .WithOne(p => p.SystemCheckHistory)
                .HasForeignKey(p => p.SystemCheckHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Folders)
                .WithOne(f => f.SystemCheckHistory)
                .HasForeignKey(f => f.SystemCheckHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.FolderChanges)
                .WithOne(f => f.SystemCheckHistory)
                .HasForeignKey(f => f.SystemCheckHistoryId)
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

            entity.HasMany(e => e.MonitoredAddresses)
                  .WithOne(m => m.NetworkStatus)
                  .HasForeignKey(m => m.NetworkStatusId)
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

        modelBuilder.Entity<MachineConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Machine)
                .WithOne(m => m.MachineConfiguration)
                .HasForeignKey<MachineConfiguration>(e => e.MachineId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FolderMonitorConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Path).IsRequired();
            
            entity.HasOne(e => e.MachineConfiguration)
                .WithMany(m => m.MonitoredFolders)
                .HasForeignKey(e => e.MachineConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
} 