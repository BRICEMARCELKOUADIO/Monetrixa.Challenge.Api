using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Monetrixa.ChallengeApp.Application.Common.Options;
using Monetrixa.ChallengeApp.Application.DTOs.Ideas;
using Monetrixa.ChallengeApp.Application.Interfaces.Ideas;
using Monetrixa.ChallengeApp.Domain.Enums;

namespace Monetrixa.ChallengeApp.Infrastructure.Services.Ideas;

public class OpenAiIdeaGenerationAiService : IIdeaGenerationAiService
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _aiOptions;

    public OpenAiIdeaGenerationAiService(
        HttpClient httpClient,
        IOptions<AiOptions> aiOptions)
    {
        _httpClient = httpClient;
        _aiOptions = aiOptions.Value;
    }

    public AiProvider Provider => AiProvider.OpenAI;

    public async Task<List<AiGeneratedIdeaResult>> GenerateIdeasAsync(
        string topic,
        MoodType mood,
        PlatformType platform,
        string challengeTheme,
        string? challengeQuote,
        CancellationToken cancellationToken = default)
    {
        var prompt = $@"
            You are a content strategy assistant.

            Generate exactly 4 content ideas.
            Return valid JSON only.
            Each item must contain:
            - title
            - content

            Context:
            - Topic: {topic}
            - Mood: {mood}
            - Platform: {platform}
            - Challenge theme: {challengeTheme}
            - Challenge quote: {challengeQuote}

            Constraints:
            - Ideas must be practical
            - Ideas must fit the selected platform
            - Tone should adapt to the mood
            - Keep them concise and actionable

            Expected JSON format:
            [
              {{
                ""title"": ""Idea title"",
                ""content"": ""Idea description""
              }}
            ]";

        var request = new OpenAiResponsesRequest
        {
            Model = _aiOptions.Model,
            Input = prompt
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "responses");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _aiOptions.ApiKey);

        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<OpenAiResponsesResponse>(cancellationToken: cancellationToken);

        if (responseBody is null)
        {
            throw new InvalidOperationException("Réponse OpenAI vide.");
        }

        var rawText = responseBody.OutputText;

        if (string.IsNullOrWhiteSpace(rawText))
        {
            throw new InvalidOperationException("Aucune idée générée par OpenAI.");
        }

        var ideas = System.Text.Json.JsonSerializer.Deserialize<List<AiGeneratedIdeaResult>>(rawText);

        if (ideas is null || ideas.Count == 0)
        {
            throw new InvalidOperationException("Impossible de parser les idées générées par OpenAI.");
        }

        return ideas;
    }

    private sealed class OpenAiResponsesRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public string Input { get; set; } = string.Empty;
    }

    private sealed class OpenAiResponsesResponse
    {
        [JsonPropertyName("output_text")]
        public string? OutputText { get; set; }
    }
}