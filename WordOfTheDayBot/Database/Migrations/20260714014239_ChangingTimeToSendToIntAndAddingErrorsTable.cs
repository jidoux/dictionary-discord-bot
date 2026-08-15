using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WordOfTheDayBot.Database.Migrations;

/// <inheritdoc />
internal partial class ChangingTimeToSendToIntAndAddingErrorsTable : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropColumn(
			name: "TimeToSendDailyWordUTC",
			schema: "wordbot",
			table: "Servers");

		migrationBuilder.AddColumn<int>(
			name: "HourToSendDailyWordUTC",
			schema: "wordbot",
			table: "Servers",
			type: "integer",
			nullable: false,
			defaultValue: 0);

		migrationBuilder.CreateTable(
			name: "Errors",
			schema: "wordbot",
			columns: table => new {
				Id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				Exception = table.Column<string>(type: "text", nullable: false),
				AdditionalMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
				EnterDateTimeUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
			},
			constraints: table => {
				table.PrimaryKey("PK_Errors", x => x.Id);
			});
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropTable(
			name: "Errors",
			schema: "wordbot");

		migrationBuilder.DropColumn(
			name: "HourToSendDailyWordUTC",
			schema: "wordbot",
			table: "Servers");

		migrationBuilder.AddColumn<TimeOnly>(
			name: "TimeToSendDailyWordUTC",
			schema: "wordbot",
			table: "Servers",
			type: "time without time zone",
			nullable: false,
			defaultValue: new TimeOnly(0, 0, 0));
	}
}
