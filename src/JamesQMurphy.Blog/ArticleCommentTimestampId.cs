using System;
using System.Collections.Generic;
using System.Linq;

namespace JamesQMurphy.Blog
{
    public class ArticleCommentTimestampId : IComparable<ArticleCommentTimestampId>
    {
        private const char SEPARATOR = '_';
        private readonly List<string> _components = new List<string>();

        public ArticleCommentTimestampId(string timestampId)
        {
            if (!string.IsNullOrEmpty(timestampId))
            {
                _components.AddRange(timestampId.Split(new char[] { SEPARATOR }));
            }
        }

        public ArticleCommentTimestampId(DateTime timestamp, string replyToId = "")
        {
            _components.Add(timestamp.ToString("O"));
            if (!string.IsNullOrEmpty(replyToId))
            {
                _components.AddRange(replyToId.Split(new char[] { SEPARATOR }).Reverse());
            }
        }

        public string CommentId => _JoinComponents(0);
        public string ReplyToId => _JoinComponents(1);

        private string _JoinComponents(int stopAt)
        {
            var retVal = "";
            if (_components.Count > stopAt)
            {
                retVal = _components[_components.Count - 1];
            }
            for (int i = _components.Count - 2; i >= stopAt; i--)
            {
                retVal = $"{retVal}{SEPARATOR}{_components[i]}";
            }
            return retVal.Replace(':','-').Replace('.','-');

        }

        public DateTime TimeStamp
        {
            get
            {
                if (_components.Count > 0)
                {
                    return DateTime.Parse(_components[0]).ToUniversalTime();
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        public string TimeStampString
        {
            get
            {
                if (_components.Count > 0)
                {
                    return _components[0];
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public override string ToString()
        {
            return string.Join($"{SEPARATOR}", _components);
        }

        public int CompareTo(ArticleCommentTimestampId other)
        {
            return TimeStampString.CompareTo(other?.TimeStampString ?? "");
        }
    }
}
