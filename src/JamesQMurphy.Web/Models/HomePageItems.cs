using JamesQMurphy.Blog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models
{
    public class HomePageItems
    {
        public readonly Article Article1 = null;
        public readonly Article Article2 = null;
        private static string[] LineSplit = new string[1] { Environment.NewLine };

        public HomePageItems(Article article1, Article article2)
        {
            Article1 = article1;
            Article2 = article2;
        }

        public static string GetArticleTeaserMarkdown(Article article)
        {
            var sbTeaser = new StringBuilder();
            var contentLines = article.Content.Split(LineSplit, StringSplitOptions.None);
            foreach (var line in contentLines)
            {
                if (line.StartsWith("#"))
                {
                    break;
                }
                sbTeaser.AppendLine(line);
            }

            return sbTeaser.ToString();
        }
    }
}
