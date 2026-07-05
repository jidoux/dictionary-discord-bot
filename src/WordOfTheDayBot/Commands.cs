using NetCord;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace WordOfTheDayBot;

// For now not touching UserCommand or MessageCommand just cuz can't think of any reason I'd need it.
public sealed class Commands(DatabaseInterface databaseInterface) : ApplicationCommandModule<ApplicationCommandContext> {
	[SlashCommand("pong", "Pong!")]
	public static string Pong() => "Ping!";

	[SlashCommand("initbot", "Set the channel and daily send time for word-of-the-day messages")]
	[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
	public async Task<string> InitBot([SlashCommandParameter(
		Description = "Channel to send the word of the day in")] TextChannel channel,
		[SlashCommandParameter(Description = "Hour to send at, in UTC (0-23)", MinValue = 0, MaxValue = 23)] int hourUtc
	) {
		ulong guildId = Context.Guild!.Id; // TODO why is this nullable ??

		await databaseInterface.SaveServerSettings(guildId, channel.Id, hourUtc);

		return $"The word of the day will send at channel <#{channel.Id}>, at {hourUtc}:00 UTC.";
	}

}
