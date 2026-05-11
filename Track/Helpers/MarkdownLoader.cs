namespace Track.Helpers
{
    public static class MarkdownLoader
    {
        public static string Load(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Prompts", fileName);
            return File.ReadAllText(path);
        }

        public static string Replace(string template, Dictionary<string, string> values)
        {
            foreach (var kv in values)
            {
                template = template.Replace($"{{{{{kv.Key}}}}}", kv.Value);
            }

            return template;
        }
    }
}
