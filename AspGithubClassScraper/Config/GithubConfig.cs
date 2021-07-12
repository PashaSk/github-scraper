using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassScraper.AspGithubClassScraper.Config
{
    public class GithubConfig
    {
        public GithubConfig(IConfiguration configuration)
        {
            var github = configuration.GetSection("Github");
            var rep = github.GetSection("Repository");
            UserName = github.GetSection("UserName").Value;
            Token = github.GetSection("Token").Value;
            RepositoryOwnerName = rep.GetSection("OwnerName").Value;
            RepositoryName = rep.GetSection("RepositoryName").Value;
            DirectoryBlackList = rep.GetSection("DirectoryBlackList").Get<string[]>();
        }

        public string UserName { get; private set; }
        public string Token { get; private set; }
        public string RepositoryOwnerName { get; private set; }
        public string RepositoryName { get; private set; }
        public string[] DirectoryBlackList { get; private set; }
    }
}
