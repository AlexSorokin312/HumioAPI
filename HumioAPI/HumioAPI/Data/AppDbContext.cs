using HumioAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HumioAPI.Data;

public class AppDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    long,
    IdentityUserClaim<long>,
    IdentityUserRole<long>,
    IdentityUserLogin<long>,
    IdentityRoleClaim<long>,
    IdentityUserToken<long>>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<UserDevice> UsersDevices => Set<UserDevice>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<AdminAccessHistory> AdminAccessHistory => Set<AdminAccessHistory>();
    public DbSet<Promocode> Promocodes => Set<Promocode>();
    public DbSet<PromocodeUsage> PromocodeUsages => Set<PromocodeUsage>();
    public DbSet<UserModuleAccess> UserModuleAccess => Set<UserModuleAccess>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentity(builder);
        ConfigureDevices(builder);
        ConfigureModules(builder);
        ConfigureProducts(builder);
        ConfigurePurchases(builder);
        ConfigureAdminAccessHistory(builder);
        ConfigurePromocodes(builder);
        ConfigurePromocodeUsages(builder);
        ConfigureUserModuleAccess(builder);
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("users");
            b.Property(u => u.CreatedAt).IsRequired();
            b.Property(u => u.Email).IsRequired();
            b.HasIndex(u => u.Email).IsUnique();
            b.HasIndex(u => u.NormalizedEmail).IsUnique();
            b.HasIndex(u => u.NormalizedUserName).IsUnique();
        });

        builder.Entity<ApplicationRole>(b => b.ToTable("roles"));
        builder.Entity<IdentityUserRole<long>>(b => b.ToTable("user_roles"));
        builder.Entity<IdentityUserClaim<long>>(b => b.ToTable("user_claims"));
        builder.Entity<IdentityRoleClaim<long>>(b => b.ToTable("role_claims"));
        builder.Entity<IdentityUserLogin<long>>(b => b.ToTable("user_logins"));
        builder.Entity<IdentityUserToken<long>>(b => b.ToTable("user_tokens"));
    }

    private static void ConfigureDevices(ModelBuilder builder)
    {
        builder.Entity<Device>(b =>
        {
            b.ToTable("devices");
            b.HasKey(d => d.Id);
            b.Property(d => d.DeviceKey).IsRequired();
            b.Property(d => d.Platform).IsRequired();
            b.Property(d => d.CreatedAt).IsRequired();

            b.HasIndex(d => d.DeviceKey).IsUnique();
        });

        builder.Entity<UserDevice>(b =>
        {
            b.ToTable("users_devices");
            b.HasKey(ud => new { ud.UserId, ud.DeviceId });

            b.Property(ud => ud.LinkedAt).IsRequired();

            b.HasOne(ud => ud.User)
                .WithMany(u => u.UserDevices)
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ud => ud.Device)
                .WithMany(d => d.UserDevices)
                .HasForeignKey(ud => ud.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureModules(ModelBuilder builder)
    {
        builder.Entity<Module>(b =>
        {
            b.ToTable("modules", table =>
            {
                table.HasCheckConstraint("ck_modules_interval_count_positive", "interval_count > 0");
            });
            b.HasKey(m => m.Id);
            b.Property(m => m.Name).IsRequired();
            b.Property(m => m.IntervalCount).IsRequired();
        });
    }

    private static void ConfigureProducts(ModelBuilder builder)
    {
        builder.Entity<Product>(b =>
        {
            b.ToTable("products");
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired();

            b.HasOne(p => p.Module)
                .WithMany(m => m.Products)
                .HasForeignKey(p => p.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePurchases(ModelBuilder builder)
    {
        builder.Entity<Purchase>(b =>
        {
            b.ToTable("purchases", table =>
            {
                table.HasCheckConstraint("ck_purchases_amount_cents_non_negative", "amount_cents >= 0");
                table.HasCheckConstraint("ck_purchases_days_positive", "days > 0");
                table.HasCheckConstraint("ck_purchases_status", "status IN ('pending','paid','failed','refunded')");
            });
            b.HasKey(p => p.Id);

            b.Property(p => p.Currency)
                .IsRequired()
                .HasColumnType("char(3)")
                .HasMaxLength(3);
            b.Property(p => p.Provider).IsRequired();
            b.Property(p => p.ProviderPaymentId).IsRequired();
            b.Property(p => p.Receipt);
            b.Property(p => p.Status)
                .HasConversion(GetPaymentStatusConverter())
                .IsRequired();
            b.Property(p => p.Days).IsRequired();
            b.Property(p => p.CreatedAt).IsRequired();

            b.HasIndex(p => new { p.Provider, p.ProviderPaymentId }).IsUnique();

            b.HasOne(p => p.User)
                .WithMany(u => u.Purchases)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.Product)
                .WithMany(pr => pr.Purchases)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAdminAccessHistory(ModelBuilder builder)
    {
        builder.Entity<AdminAccessHistory>(b =>
        {
            b.ToTable("admin_access_history", table =>
            {
                table.HasCheckConstraint("ck_admin_access_history_days_positive", "days > 0");
            });
            b.HasKey(a => a.Id);
            b.Property(a => a.Days).IsRequired();
            b.Property(a => a.CreatedAt).IsRequired();

            b.HasOne(a => a.Admin)
                .WithMany(u => u.AdminAccessEntries)
                .HasForeignKey(a => a.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(a => a.TargetUser)
                .WithMany(u => u.TargetAccessEntries)
                .HasForeignKey(a => a.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(a => a.Product)
                .WithMany(p => p.AdminAccessHistory)
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePromocodes(ModelBuilder builder)
    {
        builder.Entity<Promocode>(b =>
        {
            b.ToTable("promocodes", table =>
            {
                table.HasCheckConstraint("ck_promocodes_max_usage_count_positive", "max_usage_count > 0");
                table.HasCheckConstraint("ck_promocodes_days_positive", "days > 0");
            });
            b.HasKey(p => p.Id);
            b.Property(p => p.Code).IsRequired();
            b.Property(p => p.MaxUsageCount).IsRequired();
            b.Property(p => p.Days).IsRequired();

            b.HasIndex(p => p.Code).IsUnique();

            b.HasOne(p => p.Product)
                .WithMany(pr => pr.Promocodes)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePromocodeUsages(ModelBuilder builder)
    {
        builder.Entity<PromocodeUsage>(b =>
        {
            b.ToTable("promocode_usages");
            b.Property(pu => pu.UsedAt).IsRequired();

            b.HasKey(pu => new { pu.UserId, pu.PromocodeId });

            b.HasOne(pu => pu.User)
                .WithMany(u => u.PromocodeUsages)
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(pu => pu.Promocode)
                .WithMany(p => p.Usages)
                .HasForeignKey(pu => pu.PromocodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureUserModuleAccess(ModelBuilder builder)
    {
        builder.Entity<UserModuleAccess>(b =>
        {
            b.ToTable("user_module_access");
            b.Property(uma => uma.EndsAt).IsRequired();

            b.HasKey(uma => new { uma.UserId, uma.ModuleId });

            b.HasOne(uma => uma.User)
                .WithMany(u => u.ModuleAccesses)
                .HasForeignKey(uma => uma.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(uma => uma.Module)
                .WithMany(m => m.UserModuleAccesses)
                .HasForeignKey(uma => uma.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static ValueConverter<PaymentStatus, string> GetPaymentStatusConverter() =>
        new(
            value => value.ToString().ToLowerInvariant(),
            value => Enum.Parse<PaymentStatus>(value, true));
}
