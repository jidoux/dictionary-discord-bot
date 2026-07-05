using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WordOfTheDayBot.Database;

public class Server {
	public int Id { get; init; }
	public ulong DiscordGuildId { get; init; }
	public TimeOnly TimeToSendDailyWordUTC { get; set; }
	public DateTime EnterDateTimeUTC { get; }
}

internal class ServerConfiguration : IEntityTypeConfiguration<Server> {
	public void Configure(EntityTypeBuilder<Server> builder) {

		builder.Property(s => s.EnterDateTimeUTC)
			   .HasDefaultValueSql("timezone('utc', now())")
			   .ValueGeneratedOnAdd();

		builder.HasIndex(s => new { s.DiscordGuildId })
			.IsUnique();
	}
}
