using JamesQMurphy.Blog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models
{
    public class HomePageItems
    {
        public readonly Article Article1 = null;
        public readonly Article Article2 = null;

        public HomePageItems(Article article1, Article article2)
        {
            Article1 = article1;
            Article2 = article2;
        }
    }
}
