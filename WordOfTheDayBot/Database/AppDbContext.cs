using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace WordOfTheDayBot.Database;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	public AppDbContext() { }

	public DbSet<Server> Servers { get; set; }
	public DbSet<SentWord> SentWords { get; set; }
	public DbSet<Error> Errors { get; set; }

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
