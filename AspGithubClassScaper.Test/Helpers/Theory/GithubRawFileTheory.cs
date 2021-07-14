using System.Diagnostics.Contracts;
using Xunit;

namespace AspGithubClassScaper.Test.Helpers.Theory
{
    public class GithubRawFileTheory : TheoryData<string>
    {
        public GithubRawFileTheory(string downloadUrl)
        {
            var task = HttpClientWrapper.Instance.GetAsync(downloadUrl);
            task.Wait();
            Contract.Assert(task.Result.StatusCode == System.Net.HttpStatusCode.OK);

            var data = task.Result.Content.ReadAsStringAsync();
            data.Wait();

            Add(data.Result);
        }

    }
}
