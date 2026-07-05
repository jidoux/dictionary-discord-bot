using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace WordOfTheDayBot.Database;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	public AppDbContext() { }

	public DbSet<Server> Servers { get; set; }
	public DbSet<SentWord> SentWords { get; set; }

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
		base.ConfigureConventions(configurationBuilder);
		configurationBuilder.Properties<DateOnly>()
			.HaveConversion<DateOnlyConverter>();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		base.OnModelCreating(modelBuilder);

		modelBuilder.HasDefaultSchema("wordbot");
		// Applies configurations defined by every class which implements the generic interface IEntityTypeConfiguration
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
		base.OnConfiguring(optionsBuilder);

		if (!optionsBuilder.IsConfigured) {
			var configuration = new ConfigurationBuilder()
				.AddUserSecrets<AppDbContext>()
				.AddEnvironmentVariables()
				.Build();
			optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
		}
	}
}

public class DateOnlyConverter : ValueConverter<DateOnly, DateTime> {
	public DateOnlyConverter() : base(
		dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
		dateTime => DateOnly.FromDateTime(dateTime)) { }
}
