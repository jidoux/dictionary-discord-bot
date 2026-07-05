using NetCord.Services.ApplicationCommands;

namespace WordOfTheDayBot;

// For now not touching UserCommand or MessageCommand just cuz can't think of any reason I'd need it.
internal sealed class Commands : ApplicationCommandModule<ApplicationCommandContext> {
	[SlashCommand("pong", "Pong!")]
	public static string Pong() => "Ping!";

}
