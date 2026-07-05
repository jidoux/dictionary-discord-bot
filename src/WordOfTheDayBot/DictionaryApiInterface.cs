using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WordOfTheDayBot;

public class DictionaryApiInterface(HttpClient httpClient) {
	public async Task<DefinitionLookupResult> GetDefinitions(string word) {
		using HttpResponseMessage apiResp = await httpClient.GetAsync($"https://api.dictionaryapi.dev/api/v2/entries/en/{word}");
		if (apiResp.IsSuccessStatusCode) {
			List<DictionaryApiResponse> responseClassObj = await apiResp.Content.ReadFromJsonAsync<List<DictionaryApiResponse>>() ?? throw new Exception("Error deserializing the api response into json");
			if (responseClassObj.Count == 0) {
				return new DefinitionLookupResult.NotFound();
			}
			DictionaryApiResponse apiResponseToUse = responseClassObj[0];
			List<DefinitionAndPartOfSpeech> allDefinitions = [];
			foreach (Meanings meaning in apiResponseToUse.Meanings) {
				foreach (Definition definition in meaning.Definitions) {
					allDefinitions.Add(new DefinitionAndPartOfSpeech(
						Definition: definition.DictionaryDefinition,
						PartOfSpeech: meaning.PartOfSpeech
					));
				}
			}
			return new DefinitionLookupResult.Found(allDefinitions);
		}
		else if (apiResp.StatusCode == HttpStatusCode.NotFound) {
			return new DefinitionLookupResult.NotFound();
		}
		else {
			return new DefinitionLookupResult.Error(apiResp.ReasonPhrase ?? "No given reason phrase", apiResp.StatusCode);
		}
	}
}

public abstract record DefinitionLookupResult {
	public sealed record Found(List<DefinitionAndPartOfSpeech> Definitions) : DefinitionLookupResult;
	public sealed record NotFound : DefinitionLookupResult;
	public sealed record Error(string Reason, HttpStatusCode StatusCode) : DefinitionLookupResult;
}

// Not including some fields that I don't expect I will care about. All fields can be seen at https://dictionaryapi.dev/
public class DictionaryApiResponse {
	[JsonPropertyName("word")]
	public required string Word { get; set; }
	[JsonPropertyName("meanings")]
	public List<Meanings> Meanings { get; set; } = [];
	[JsonPropertyName("sourceUrls")]
	public List<string> SourceUrls { get; set; } = [];
}

public class Meanings {
	[JsonPropertyName("partOfSpeech")]
	public required string PartOfSpeech { get; set; }
	[JsonPropertyName("definitions")]
	public List<Definition> Definitions { get; set; } = [];
	[JsonPropertyName("synonyms")]
	public List<string> Synonyms { get; set; } = [];
	[JsonPropertyName("antonyms")]
	public List<string> Antonyms { get; set; } = [];
}

public class Definition {
	[JsonPropertyName("definition")]
	public required string DictionaryDefinition { get; set; }
	[JsonPropertyName("synonyms")]
	public List<string> Synonyms { get; set; } = [];
	[JsonPropertyName("antonyms")]
	public List<string> Antonyms { get; set; } = [];
}
