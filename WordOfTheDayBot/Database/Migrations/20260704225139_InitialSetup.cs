using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WordOfTheDayBot.Database.Migrations;

/// <inheritdoc />
public partial class InitialSetup : Migration {
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder) {
		migrationBuilder.EnsureSchema(
			name: "wordbot");

		migrationBuilder.CreateTable(
			name: "Servers",
			schema: "wordbot",
			columns: table => new {
				Id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				TimeToSendDailyWordUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				EnterDateTimeUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
			},
			constraints: table => {
				table.PrimaryKey("PK_Servers", x => x.Id);
			});

		migrationBuilder.CreateTable(
			name: "SentWords",
			schema: "wordbot",
			columns: table => new {
				Id = table.Column<int>(type: "integer", nullable: false)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
				Word = table.Column<string>(type: "text", nullable: false),
				ServerId = table.Column<int>(type: "integer", nullable: true),
				EnterDateTimeUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
			},
			constraints: table => {
				table.PrimaryKey("PK_SentWords", x => x.Id);
				table.ForeignKey(
					name: "FK_SentWords_Servers_ServerId",
					column: x => x.ServerId,
					principalSchema: "wordbot",
					principalTable: "Servers",
					principalColumn: "Id");
			});

		migrationBuilder.CreateIndex(
			name: "IX_SentWords_ServerId",
			schema: "wordbot",
			table: "SentWords",
			column: "ServerId");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder) {
		migrationBuilder.DropTable(
			name: "SentWords",
			schema: "wordbot");

		migrationBuilder.DropTable(
			name: "Servers",
			schema: "wordbot");
	}
}
