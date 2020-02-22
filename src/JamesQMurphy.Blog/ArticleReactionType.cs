using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public enum ArticleReactionType
    {
        Comment,
        Edit,
        Hide,   // soft delete
        Delete, // hard delete
        Vote
    }
}
