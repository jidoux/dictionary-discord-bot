namespace WordOfTheDayBot;

internal sealed class WordManager(DictionaryApiInterface dictionaryApiInterface) {
	private readonly Lazy<ValueTask<string[]>> _allWords = new(async () => {
		// TODO test this when its a docker image
		const string wordPoolPath = "word_pool.json";
		using StreamReader streamReader = new(wordPoolPath);
		string fileAsString = await streamReader.ReadToEndAsync();
		return JsonSerializer.Deserialize<string[]>(fileAsString) ?? throw new Exception("Could not deserialize, somehow, idk");
	});

	public async Task<WordAndDefinitions> GetWordAndAllDefinitions(CancellationToken stoppingToken) {
		string[] allWords = await _allWords.Value;
		while (true) {
			string word = allWords[Random.Shared.Next(allWords.Length)];
			List<DefinitionAndPartOfSpeech>? possibleDefinitions = await GetDefinitionsFromWord(word, stoppingToken);
			if (possibleDefinitions is not null) {
				return new WordAndDefinitions(word, possibleDefinitions);
			}
		}
	}

	// TODO test the word entangle, somewthing weird happened with the definition which idk if its my fault or not but probably is... it newlined the , or smth idk
	private async Task<List<DefinitionAndPartOfSpeech>?> GetDefinitionsFromWord(string word, CancellationToken stoppingToken) {
		DefinitionLookupResult definitionLookupResult = await dictionaryApiInterface.GetDefinitions(word, stoppingToken);
		if (definitionLookupResult is DefinitionLookupResult.Found foundDefinitions) {
			return foundDefinitions.Definitions;
		}
		else if (definitionLookupResult is DefinitionLookupResult.NotFound) {
			// The caller will interpret this as "I need to get another word".
			return null;
		}
		else {
			// Somewhere down the line this will get caught and we will just silently continue.
			throw new Exception("The API was down, for some reason");
		}

	}
}

internal record WordAndDefinitions(string Word, List<DefinitionAndPartOfSpeech> Definitions);

internal record DefinitionAndPartOfSpeech(string Definition, string PartOfSpeech);
