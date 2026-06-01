namespace Track.Services
{
    public interface IPolicyQAService
    {
        Task UploadDocumentAsync(IFormFile file);
        Task<string> AskAsync(string query);
        //Task AskStreamAsync(string query, HttpResponse response); 
    }
}