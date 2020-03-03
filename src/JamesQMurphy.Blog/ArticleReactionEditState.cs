using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public enum ArticleReactionEditState
    {
        Original,
        Edited,
        Hidden,   // soft delete
        Deleted // hard delete
    }
}
