using NetCord;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace WordOfTheDayBot;

// For now not touching UserCommand or MessageCommand just cuz can't think of any reason I'd need it.
public sealed class Commands(DatabaseInterface databaseInterface, UnexpectedErrorHandler unexpectedErrorHandler) : ApplicationCommandModule<ApplicationCommandContext> {
	//[SlashCommand("pong", "Pong!")]
	//public static string Pong() => "Ping!";

	// TODO design this command at some point since its pretty bad lol
	[SlashCommand("initbot", "Set the channel and daily send time for word-of-the-day messages")]
	[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
	public async Task<string?> InitBot(
		[SlashCommandParameter(Description = "Channel to send the word of the day in")] TextChannel channel,
		[SlashCommandParameter(Description = "Hour to send at, in UTC (0-23)", MinValue = 0, MaxValue = 23)] int hourUtc) {
		try {
			if (Context.Guild is null) {
				return "An internal error occurred, which has been logged. Please try again later";
				throw new Exception("Context.Guild is null, somehow: " + JsonSerializer.Serialize(Context));
			}
			ulong guildId = Context.Guild.Id;
			// TODO test this
			if (!Context.Guild.Channels.ContainsKey(channel.Id)) {
				return "This channel does not exist in your server, sorry!";
			}

			await databaseInterface.SaveServerSettings(guildId, channel.Id, hourUtc);

			return $"The word of the day will send at channel <#{channel.Id}>, at {hourUtc}:00 UTC.";
		}
		catch (Exception ex) {
			await unexpectedErrorHandler.HandleError(ex);
		}
		return null; // TODO does this even work yeah idek test it
	}
}
