namespace WordOfTheDayBot;

public class MessageSender {
	public async Task SendWordOfTheDayPoll(WordAndDefinitions wordAndDefinitions, ulong guildId) {
		Console.Write($"Sending word of the day poll: {JsonSerializer.Serialize(wordAndDefinitions)}, {guildId}");
	}
}
