using System;
using System.Collections.Generic;
using System.Linq;

namespace JamesQMurphy.Blog
{
    public class ArticleReactionTimestampId : IComparable<ArticleReactionTimestampId>
    {
        private const char SEPARATOR = '_';

        public static string TimestampToJQueryFriendlyString(string timestampString) => timestampString.Replace(':','c').Replace('.','p');
        public static string JQueryFriendlyToTimestampString(string jqueryFriendlyString) => jqueryFriendlyString.Replace('c', ':').Replace('p', '.');


        private readonly List<string> _pieces = new List<string>();

        public ArticleReactionTimestampId(string timestampId)
        {
            if (!string.IsNullOrEmpty(timestampId))
            {
                _pieces.AddRange(
                    JQueryFriendlyToTimestampString(timestampId)
                    .Split(new char[] { SEPARATOR })
                    );
            }
        }

        public ArticleReactionTimestampId(DateTime timestamp, string replyToId = "")
        {
            _pieces.Add(timestamp.ToString("O"));
            if (!string.IsNullOrEmpty(replyToId))
            {
                _pieces.AddRange(
                    JQueryFriendlyToTimestampString(replyToId)
                    .Split(new char[] { SEPARATOR })
                    .Reverse()
                    );
            }
        }

        public string ReactionId => _JoinPiecesInReverseOrderDownTo(0);
        public string ReactingToId => _JoinPiecesInReverseOrderDownTo(1);
        public int NestingLevel => _pieces.Count;

        private string _JoinPiecesInReverseOrderDownTo(int stopAt)
        {
            var retVal = "";
            if (_pieces.Count > stopAt)
            {
                retVal = _pieces[_pieces.Count - 1];
            }
            for (int i = _pieces.Count - 2; i >= stopAt; i--)
            {
                retVal = $"{retVal}{SEPARATOR}{_pieces[i]}";
            }
            return TimestampToJQueryFriendlyString(retVal);
        }

        public DateTime TimeStamp
        {
            get
            {
                if (_pieces.Count > 0)
                {
                    return DateTime.Parse(_pieces[0]).ToUniversalTime();
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
                if (_pieces.Count > 0)
                {
                    return _pieces[0];
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public override string ToString()
        {
            return string.Join($"{SEPARATOR}", _pieces);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ArticleReactionTimestampId;
            if (!(other is null))
            {
                return this.ToString() == other.ToString();
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(ArticleReactionTimestampId other)
        {
            return TimeStampString.CompareTo(other?.TimeStampString ?? "");
        }
    }
}
