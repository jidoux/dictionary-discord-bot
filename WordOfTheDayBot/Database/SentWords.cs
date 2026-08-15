using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordOfTheDayBot.Database;

public class SentWord {
	public int Id { get; init; }
	public required string Word { get; init; }
	[ForeignKey(nameof(Server))]
	public int? ServerId { get; init; }
	public Server? Server { get; init; }
	public DateTime EnterDateTimeUTC { get; }
}

internal class SentWordConfiguration : IEntityTypeConfiguration<SentWord> {
	public void Configure(EntityTypeBuilder<SentWord> builder) {

		builder.Property(w => w.EnterDateTimeUTC)
			   .HasDefaultValueSql("timezone('utc', now())")
			   .ValueGeneratedOnAdd();
	}
}
