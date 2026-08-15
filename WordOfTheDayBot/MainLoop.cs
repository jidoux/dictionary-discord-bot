using WordOfTheDayBot.Database;

namespace WordOfTheDayBot;

internal sealed class MainLoop(DatabaseInterface databaseInterface, WordManager wordManager, MessageSender messageSender, UnexpectedErrorHandler unexpectedErrorHandler, ILogger<MainLoop> logger) : BackgroundService {

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		try {
			while (true) {
				stoppingToken.ThrowIfCancellationRequested();
				DateTime now = DateTime.Now;
				DateTime nextHour = now.Date.AddHours(now.Hour + 1);
				TimeSpan delayUntilNextHour = nextHour - now;

				await Task.Delay(delayUntilNextHour, stoppingToken);
				// Wait an additional minute... there is a timing error where it somehow is getting the same DateTime.UtcNow.Hour 2 hours in a row
				// i.e. 11:00 and 12:00, returns 12 both times. Can also return 12 no times, 1 time, etc.
				await Task.Delay(TimeSpan.FromSeconds(50), stoppingToken);

				await DoHourlyWork(stoppingToken);
			}
		}
		catch (Exception ex) {
			await unexpectedErrorHandler.HandleError(ex, "This could just have been the cancellationToken throwing", stoppingToken: stoppingToken);
		}

	}

	private async Task DoHourlyWork(CancellationToken stoppingToken) {
		List<Server> allServersToSendToRightNow;
		WordAndDefinitions initialWordAndDefinition;

		try {
			logger.LogInformation("DoHourlyWork() called once again");

			allServersToSendToRightNow = await databaseInterface.FindAllServersToSendForThisUTCHour(stoppingToken);
			if (allServersToSendToRightNow.Count == 0) {
				return;
			}
			if (logger.IsEnabled(LogLevel.Information)) {
				logger.LogInformation("Found {ServerCount} servers to send a word to {Server}", allServersToSendToRightNow.Count, string.Join(',', allServersToSendToRightNow.Select(x => x.DiscordGuildId)));
			}
			// I want an initial one, which is shared, mostly for performance reasons, but I also like the consistency.
			initialWordAndDefinition = await wordManager.GetWordAndAllDefinitions(stoppingToken);
		}
		catch (Exception ex) {
			// I figure if this line executes, then its fine to just fail this and try again next hour.
			await unexpectedErrorHandler.HandleError(ex, stoppingToken: stoppingToken);
			return;
		}

		foreach (Server server in allServersToSendToRightNow) {
			try {
				WordAndDefinitions currentWordAndDefinition = await GetUsableWordForServer(initialWordAndDefinition, server, stoppingToken);
				// TODO - ideally this should be done atomically... but yeah idk. Maybe its not the worst thing ever, idk.
				await messageSender.SendWordOfTheDayPoll(currentWordAndDefinition, server.DiscordChannelIdToSendWordsTo, stoppingToken);
				await databaseInterface.AddSentWordToServer(currentWordAndDefinition.Word, server.Id, stoppingToken);
			}
			catch (Exception ex) {
				// This line can get executed when the dictionary API is down as I throw an exception in that case.
				// Beyond that, it would be fully unexpected.
				await unexpectedErrorHandler.HandleError(ex, stoppingToken: stoppingToken);
			}
		}
	}

	private async Task<WordAndDefinitions> GetUsableWordForServer(WordAndDefinitions candidate, Server server, CancellationToken stoppingToken) {
		const int maxAttempts = 100_000; // Just in case
		for (int i = 0; i < maxAttempts; i++) {
			bool wasWordSentHereAlready = await databaseInterface.WasWordAlreadySentInThisServer(candidate.Word, server.Id, stoppingToken);
			if (!wasWordSentHereAlready) {
				return candidate;
			}
			candidate = await wordManager.GetWordAndAllDefinitions(stoppingToken);
		}

		throw new Exception($"We tried {maxAttempts} times and could not find a good word and definitions.. this should never get executed");
	}
}
