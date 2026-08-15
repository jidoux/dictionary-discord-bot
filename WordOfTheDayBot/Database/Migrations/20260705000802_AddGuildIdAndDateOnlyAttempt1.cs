using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordOfTheDayBot.Database.Migrations;

/// <inheritdoc />
internal partial class AddGuildIdAndDateOnlyAttempt1 : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.AlterColumn<TimeOnly>(
			name: "TimeToSendDailyWordUTC",
			schema: "wordbot",
			table: "Servers",
			type: "time without time zone",
			nullable: false,
			oldClrType: typeof(DateTime),
			oldType: "timestamp with time zone");

		migrationBuilder.AddColumn<decimal>(
			name: "DiscordGuildId",
			schema: "wordbot",
			table: "Servers",
			type: "numeric(20,0)",
			nullable: false,
			defaultValue: 0m);

		migrationBuilder.CreateIndex(
			name: "IX_Servers_DiscordGuildId",
			schema: "wordbot",
			table: "Servers",
			column: "DiscordGuildId",
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropIndex(
			name: "IX_Servers_DiscordGuildId",
			schema: "wordbot",
			table: "Servers");

		migrationBuilder.DropColumn(
			name: "DiscordGuildId",
			schema: "wordbot",
			table: "Servers");

		migrationBuilder.AlterColumn<DateTime>(
			name: "TimeToSendDailyWordUTC",
			schema: "wordbot",
			table: "Servers",
			type: "timestamp with time zone",
			nullable: false,
			oldClrType: typeof(TimeOnly),
			oldType: "time without time zone");
	}
}
