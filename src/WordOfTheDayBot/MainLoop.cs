using WordOfTheDayBot.Database;

namespace WordOfTheDayBot;

public sealed class MainLoop(DatabaseInterface databaseInterface, WordManager wordManager, MessageSender messageSender, UnexpectedErrorHandler unexpectedErrorHandler) {
	public async Task RunHourly() {
		while (true) {
			DateTime now = DateTime.Now;
			DateTime nextHour = now.Date.AddHours(now.Hour + 1);
			TimeSpan delayUntilNextHour = nextHour - now;

			//await Task.Delay(delayUntilNextHour);
			await Task.Delay(10000); // TODO DELETEME 3 (there are 2 more of these, indubitably.

			await DoHourlyWork();
		}
	}

	private async Task DoHourlyWork() {
		int currentHourUTC = DateTime.UtcNow.Hour;
		List<Server> allServersToSendToRightNow;
		WordAndDefinitions initialWordAndDefinition;

		try {
			allServersToSendToRightNow = await databaseInterface.FindAllServersToSendForThisUTCHour(currentHourUTC);
			// I want an initial one, which is shared, mostly for performance reasons, but I also like the consistency.
			initialWordAndDefinition = await wordManager.GetWordAndAllDefinitions();
		}
		catch (Exception ex) {
			// I figure if this line executes, then its fine to just fail this and try again next hour.
			await unexpectedErrorHandler.HandlerError(ex);
			return;
		}

		foreach (Server server in allServersToSendToRightNow) {
			try {
				WordAndDefinitions currentWordAndDefinition = await GetUsableWordForServer(initialWordAndDefinition, server);
				// TODO - ideally this should be done atomically... but yeah idk.
				await messageSender.SendWordOfTheDayPoll(currentWordAndDefinition, server.DiscordChannelIdToSendWordsTo);
				await databaseInterface.AddSentWordToServer(currentWordAndDefinition.Word, server.Id);
			}
			catch (Exception ex) {
				// This line can get executed when the dictionary API is down as I throw an exception in that case.
				// Beyond that, it would be fully unexpected.
				await unexpectedErrorHandler.HandlerError(ex);
			}
		}
	}

	private async Task<WordAndDefinitions> GetUsableWordForServer(WordAndDefinitions candidate, Server server) {
		const int maxAttempts = 100_000; // Just in case
		for (int i = 0; i < maxAttempts; i++) {
			bool wasWordSentHereAlready = await databaseInterface.WasWordAlreadySentInThisServer(candidate.Word, server.Id);
			if (!wasWordSentHereAlready) {
				return candidate;
			}
			candidate = await wordManager.GetWordAndAllDefinitions();
		}

		throw new Exception($"We tried {maxAttempts} times and could not find a good word and definitions.. this should never get executed");
	} 
}
