namespace Track.AI
{
    public interface IAIClient
    {
        Task<string> GetCompletionAsync(string prompt);
        IAsyncEnumerable<string> GetCompletionStreamAsync(string prompt);

    }
}
