using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WordOfTheDayBot;

public class DictionaryApiInterface(HttpClient httpClient) {
	public async Task<DefinitionLookupResult> GetDefinitions(string word, CancellationToken stoppingToken) {
		UriBuilder uriBuilder = new("https", "api.dictionaryapi.dev") {
			Scheme = "https",
			Path = "api/v2/entries/en/",
			Query = word,
		};
		_ = uriBuilder.Uri;
		using HttpResponseMessage apiResp = await httpClient.GetAsync(uriBuilder.Uri, stoppingToken);
		if (apiResp.IsSuccessStatusCode) {
			List<DictionaryApiResponse> responseClassObj = await apiResp.Content.ReadFromJsonAsync<List<DictionaryApiResponse>>(stoppingToken) ?? throw new Exception($"Error deserializing the api response into json with word {word}");
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

// Not including some fields that I don't expect I will care about.
public class DictionaryApiResponse {
	[JsonPropertyName("word")]
	public required string Word { get; set; }
	[JsonPropertyName("meanings")]
	public IReadOnlyCollection<Meanings> Meanings { get; set; } = [];
	[JsonPropertyName("sourceUrls")]
	public IReadOnlyCollection<string> SourceUrls { get; set; } = [];
}

public class Meanings {
	[JsonPropertyName("partOfSpeech")]
	public required string PartOfSpeech { get; set; }
	[JsonPropertyName("definitions")]
	public IReadOnlyCollection<Definition> Definitions { get; set; } = [];
	[JsonPropertyName("synonyms")]
	public IReadOnlyCollection<string> Synonyms { get; set; } = [];
	[JsonPropertyName("antonyms")]
	public IReadOnlyCollection<string> Antonyms { get; set; } = [];
}

public class Definition {
	[JsonPropertyName("definition")]
	public required string DictionaryDefinition { get; set; }
	[JsonPropertyName("synonyms")]
	public IReadOnlyCollection<string> Synonyms { get; set; } = [];
	[JsonPropertyName("antonyms")]
	public IReadOnlyCollection<string> Antonyms { get; set; } = [];
}
