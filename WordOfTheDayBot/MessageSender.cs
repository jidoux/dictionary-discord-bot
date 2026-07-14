using NetCord;
using NetCord.Rest;
using System.Text;

namespace WordOfTheDayBot;

public class MessageSender(RestClient restClient) {
	public async Task SendWordOfTheDayPoll(WordAndDefinitions wordAndDefinitions, ulong channelId, CancellationToken stoppingToken) {
		StringBuilder definitionsText = new();
		foreach (DefinitionAndPartOfSpeech definitionAndPartOfSpeech in wordAndDefinitions.Definitions) {
			definitionsText.AppendLine($"(*{definitionAndPartOfSpeech.PartOfSpeech}*): {definitionAndPartOfSpeech.Definition}");
		}
		var poll = new MessagePollProperties(
			question: new MessagePollMediaProperties { Text = $"Did you know the word {wordAndDefinitions.Word}?" },
			answers: [
				new MessagePollAnswerProperties(new MessagePollMediaProperties() {
					Text = "Yes"
				}),
				new MessagePollAnswerProperties(new MessagePollMediaProperties() {
					Text = "No"
				}),
				new MessagePollAnswerProperties(new MessagePollMediaProperties() {
					Text = "Idk"
				})
				]
			)
			.WithDurationInHours(23);
		await restClient.SendMessageAsync(channelId, new MessageProperties {
			Content = $"The word of the day is: **{wordAndDefinitions.Word}**\n||{definitionsText}||",
			Poll = poll
		}, cancellationToken: stoppingToken);
	}
}
