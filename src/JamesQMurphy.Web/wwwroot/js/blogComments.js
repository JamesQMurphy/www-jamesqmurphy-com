$(function () {
    $("#commentsSection").append(BlogComments.HtmlForMoreBlock('', 'SHOW MORE COMMENTS'));
    BlogComments.FetchLatestComments();
});

function BlogComments() { }


BlogComments.lastTimestampRetrieved = '';

BlogComments.FetchLatestComments = function () {
    $.getJSON(document.URL + '/comments?sinceTimestamp=' + BlogComments.lastTimestampRetrieved, function (commentsArray) {
        if (commentsArray.length > 0) {
            BlogComments.InsertCommentsIntoDOM(commentsArray);
            BlogComments.lastTimestampRetrieved = commentsArray[commentsArray.length - 1].timestamp;
            setTimeout(BlogComments.FetchLatestComments, 6000);
        }
        else {
            setTimeout(BlogComments.FetchLatestComments, 6000);
        }
    });
};

BlogComments.InsertCommentsIntoDOM = function (commentsArray) {
    console.log("inside BlogComments.InsertCommentsIntoDOM, commentsArray = " + commentsArray.length);
    $.each(commentsArray, function (index, blogArticleComment) {
        var parentId = blogArticleComment.replyToId;
        if (parentId.length === 0) {
            parentId = "commentsSection";
        }

        console.log("Looking under ID " + parentId);
        console.log("replyToId is " + blogArticleComment.replyToId);
        var insertBeforeElement = $("#" + parentId + " .media[id][data-timestamp]")
            .filter(function () {
                console.log('timestamp is ' + $(this).data('timestamp'));
                console.log('id is ' + $(this).attr('id'));
                return ($(this).data('timestamp') > blogArticleComment.timestamp)
                    && ($(this).attr('id').startsWith(blogArticleComment.replyToId + '/'));
            })
            .first();

        console.log(insertBeforeElement.length);

        $('<div class="jqm-comment media my-3" id="' + blogArticleComment.commentId + '" data-timestamp="' + blogArticleComment.timestamp + '">' +
            '<div class="jqm-comment-user-icon">' +
                '<img class="img-fluid" src="' + blogArticleComment.authorImageUrl + '">' +
            '</div>' +
            '<div class="jqm-comment media-body px-3">' +
                '<b>' + blogArticleComment.authorName + '</b> ' + (blogArticleComment.isMine ? '(you)' : '') + '<br/>' +
                blogArticleComment.htmlContent +
            BlogComments.HtmlForMoreBlock(blogArticleComment.commentId, 'VIEW REPLIES') +
            '</div>' + 
          '</div>').insertBefore(insertBeforeElement);
    });
};

BlogComments.HtmlForMoreBlock = function (id, viewText) {
    return '<div class="media my-3" id="' + id + '/more" data-timestamp="more">' +
        '<span id="' + id + '/view">' + viewText + '</span>&nbsp;</div>';
};


if (!String.prototype.startsWith) {
    Object.defineProperty(String.prototype, 'startsWith', {
        value: function (search, rawPos) {
            var pos = rawPos > 0 ? rawPos | 0 : 0;
            return this.substring(pos, pos + search.length) === search;
        }
    });
}


