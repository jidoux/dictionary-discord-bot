using WordOfTheDayBot.Database;

namespace WordOfTheDayBot;

public sealed class MainLoop(DatabaseInterface databaseInterface, WordManager wordManager, MessageSender messageSender) {
	public async Task RunHourly() {
		while (true) {
			DateTime now = DateTime.Now;
			DateTime nextHour = now.Date.AddHours(now.Hour + 1);
			TimeSpan delayUntilNextHour = nextHour - now;

			//await Task.Delay(delayUntilNextHour);
			await Task.Delay(10000);

			await DoHourlyWork();
		}
	}

	private async Task DoHourlyWork() {
		int currentHourUTC = DateTime.UtcNow.Hour;
		List<Server> allServersToSendToRightNow = await databaseInterface.FindAllServersToSendForThisUTCHour(currentHourUTC);

		WordAndDefinitions wordToTry = await wordManager.GetWordAndAllDefinitions();
		foreach (Server server in allServersToSendToRightNow) {
			if (!await databaseInterface.WasWordAlreadySentInThisServer(wordToTry.Word, server.Id)) {
				await messageSender.SendWordOfTheDayPoll(wordToTry, server.DiscordGuildId);
			}
			else {
				while (true) {
					WordAndDefinitions nextWordToTry = await wordManager.GetWordAndAllDefinitions();
					if (!await databaseInterface.WasWordAlreadySentInThisServer(nextWordToTry.Word, server.Id)) {
						await messageSender.SendWordOfTheDayPoll(nextWordToTry, server.DiscordGuildId);
						break;
					}
				}
			}
		}
	}
}
