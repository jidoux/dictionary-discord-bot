using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace WordOfTheDayBot.Database;

public class Error {
	public int Id { get; init; }
	public required string Exception { get; init; }
	[MaxLength(2048)]
	public string? AdditionalMessage { get; init; }
	public DateTime EnterDateTimeUTC { get; }
}

internal class ErrorConfiguration : IEntityTypeConfiguration<Error> {
	public void Configure(EntityTypeBuilder<Error> builder) {

		builder.Property(s => s.EnterDateTimeUTC)
			   .HasDefaultValueSql("timezone('utc', now())")
			   .ValueGeneratedOnAdd();
	}
}
