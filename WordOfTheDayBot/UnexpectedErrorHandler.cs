namespace WordOfTheDayBot;

public sealed class UnexpectedErrorHandler(ILogger<UnexpectedErrorHandler> logger) {
	public async Task HandleError(Exception ex, string? additionalMessage = null) {
		// I figure I should probably guarantee this never throws
		try {
			logger.LogError(ex, "Additional message: {AdditionalMessage}", additionalMessage);
		}
		catch {
			// at this point I don't care if something throws.
		}
	}
}
