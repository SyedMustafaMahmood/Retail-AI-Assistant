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

            // 🔥 IMPORTANT: Show real error if API fails
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API Error ({response.StatusCode}): {json}");
            }

            using var doc = JsonDocument.Parse(json);

            // 🔥 SAFE parsing (prevents runtime crashes)
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
    }
}