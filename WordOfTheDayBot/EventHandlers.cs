using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using Serilog.Core;

namespace WordOfTheDayBot;

/*
Few notes:
1.) The handlers need to be public, not internal. So, sadly it forces everything to be public... oh well.
2.) Somehow you don't need to register the handlers with DI, I guess they are registered as
some kind of entry point already, which is nice.
*/

//public sealed class MessageCreateHandler : IMessageCreateGatewayHandler {

// This fires when the bot starts up, or when the bot joins a given server.
public sealed class GuildJoinHandler(DatabaseInterface databaseInterface, ILogger<GuildJoinHandler> logger) : IGuildCreateGatewayHandler {
	public async ValueTask HandleAsync(GuildCreateEventArgs arg) {
		logger.LogDebug("GuildJoinHandler called");
		await databaseInterface.AddServerIfNotExists(arg.GuildId);
	}
}

// This seems reasonable enough. Not foolproof, but whatever.
public sealed class GuildExitHandler(DatabaseInterface databaseInterface, ILogger<GuildExitHandler> logger) : IGuildDeleteGatewayHandler {
	public async ValueTask HandleAsync(GuildDeleteEventArgs arg) {
		logger.LogDebug("GuildExitHandler called");
		await databaseInterface.DeleteServerByGuildId(arg.GuildId);
	}
}

public sealed class ReadyHandler(MainLoop mainLoop, ILogger<ReadyHandler> logger) : IReadyGatewayHandler {
	public async ValueTask HandleAsync(ReadyEventArgs arg) {
		logger.LogDebug("ReadyHandler called");
		await mainLoop.RunHourly();
	}
}
