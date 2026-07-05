namespace WordOfTheDayBot;

public sealed class WordManager {
	private readonly Lazy<ValueTask<string[]>> _allWords = new(async () => {
		// TODO will the directory be different when this is a docker image??
		using StreamReader streamReader = new(@"..\..\word_pool.txt");
		string fileAsString = await streamReader.ReadToEndAsync();
		return JsonSerializer.Deserialize<string[]>(fileAsString) ?? throw new Exception("Could not deserialize, somehow, idk");
	});

	public async Task<WordAndDefinitions> GetWordAndAllDefinitions() {
		string[] allWords = await _allWords.Value;
		string word = allWords[Random.Shared.Next(allWords.Length)];
		List<string> definitions = [];
		return new WordAndDefinitions(word, definitions);
	}
}

public record WordAndDefinitions(string Word, List<string> Definitions);
