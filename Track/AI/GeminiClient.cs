using System.Text;
using System.Text.Json;

namespace Track.AI
{
    public class GeminiClient : IAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GeminiClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            var apiKey = _config["Gemini:ApiKey"];

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);

            var json = await response.Content.ReadAsStringAsync();

            // Show real error if API fails
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API Error ({response.StatusCode}): {json}");
            }

            using var doc = JsonDocument.Parse(json);

            // SAFE parsing 
            if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
            {
                throw new Exception("Invalid Gemini response: " + json);
            }

            var text = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "No response from Gemini";
        }
        public async IAsyncEnumerable<string> GetCompletionStreamAsync(string prompt)
        {
            var apiKey = _config["Gemini:ApiKey"];

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:streamGenerateContent?alt=sse&key={apiKey}";

            var body = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json")
            };

            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Gemini Streaming Error ({response.StatusCode}): {error}");
            }

            var stream = await response.Content.ReadAsStreamAsync();

            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (!line.StartsWith("data: "))
                    continue;

                var json = line.Substring(6);

                string? text = null;

                try
                {
                    using var doc = JsonDocument.Parse(json);

                    var root = doc.RootElement;

                    if (!root.TryGetProperty("candidates", out var candidates))
                        continue;

                    text = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();
                }
                catch
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
    }
}
