namespace WordOfTheDayBot;

public sealed class UnexpectedErrorHandler(ILogger<UnexpectedErrorHandler> logger, DatabaseInterface databaseInterface) {
	public async Task HandleError(Exception ex, string? additionalMessage = null, CancellationToken stoppingToken = default) {
		// I figure I should probably guarantee this never throws...
		try {
			await databaseInterface.InsertError(ex, additionalMessage, stoppingToken);
		}
		catch (Exception ex2) {
			AggregateException exToShow = new(ex, ex2);
			try {
#pragma warning disable S6667 // I figure I am logging the exception just not in a direct sorta way. Not sure if there's a better approach to this, though.
				logger.LogCritical(exToShow, additionalMessage);
#pragma warning restore S6667
			}
			catch {
				// worst case scenario ahahaha
			}
		}
		try {
			logger.LogError(ex, "Additional message: {AdditionalMessage}", additionalMessage);
		}
		catch {
			// I don't care if something throws at this point
		}
	}
}
