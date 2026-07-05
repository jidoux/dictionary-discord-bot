namespace WordOfTheDayBot;

public sealed class UnexpectedErrorHandler {
	public async Task HandlerError(Exception ex, string? additionalMessage = null) {
		// I figure I should probably guarantee this never throws
		try {
			Console.WriteLine("Unexpected error occurred: " + ex);

		}
		catch (Exception exVeryBad) {
			Console.Error.WriteLine(exVeryBad);
		}
	}
}
