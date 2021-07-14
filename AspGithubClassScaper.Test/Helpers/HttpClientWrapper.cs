using System.Net.Http;

namespace AspGithubClassScaper.Test.Helpers
{
    public static class HttpClientWrapper
    {
        public static HttpClient Instance => _instance;
        private static HttpClient _instance;

        static HttpClientWrapper()
        {
            _instance = new HttpClient();
        }
    }
}
