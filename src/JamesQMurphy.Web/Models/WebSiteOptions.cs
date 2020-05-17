using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models
{
    public class WebSiteOptions
    {
        public string WebSiteTitle { get; set; } = "Website";
        public string AppName { get; set; } = "JamesQMurphyWeb-local";
        public string DataProtection { get; set; } = "";
        public string TwitterAccount { get; set; } = "JamesQMurphy";
        public string GithubAccount { get; set; } = "JamesQMurphy";
        public string SiteUrl {get; set; } = "";
        public string ImageBasePath { get; set; } = "";
        public int ArticlesInRss { get; set; } = 1;
        public int CommentNestLevels { get; set; } = 2;

        public string TwitterLink => $"https://twitter.com/{TwitterAccount}?ref_src=twsrc%5Etfw";
        public string GithubLink => $"https://github.com/{GithubAccount}";
    }
}
