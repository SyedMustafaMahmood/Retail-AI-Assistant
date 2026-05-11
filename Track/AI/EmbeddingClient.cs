using System.Text;
using System.Text.Json;

namespace Track.AI
{
    public class EmbeddingClient : IEmbeddingClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public EmbeddingClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var apiKey = _config["Gemini:ApiKey"];

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={apiKey}";
            var body = new
            {
                content = new
                {
                    parts = new[]
                    {
                        new { text = text }
                    }
                }
            };

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            );

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            using var doc = JsonDocument.Parse(json);

            var values = doc.RootElement
                .GetProperty("embedding")
                .GetProperty("values")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            return values;
        }
    }

}
