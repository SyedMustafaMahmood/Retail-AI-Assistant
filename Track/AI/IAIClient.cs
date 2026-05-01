namespace Track.AI
{
    // AI/IAIClient.cs
    public interface IAIClient
    {
        Task<string> GetCompletionAsync(string prompt);
    }
}
