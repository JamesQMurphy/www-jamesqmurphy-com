$(function () {
    BlogComments.FetchLatestComments();
});

function BlogComments() { }


BlogComments.lastTimestampRetrieved = '';

BlogComments.FetchLatestComments = function () {
    $.getJSON(document.URL + '/comments?sinceTimestamp=' + BlogComments.lastTimestampRetrieved, function (commentsArray) {
        if (commentsArray.length > 0) {
            BlogComments.InsertCommentsIntoDOM(commentsArray);
            BlogComments.lastTimestampRetrieved = commentsArray[commentsArray.length - 1].timestamp;
            setTimeout(BlogComments.FetchLatestComments, 15000);
        }
        else {
            setTimeout(BlogComments.FetchLatestComments, 15000);
        }
    });
};

BlogComments.InsertCommentsIntoDOM = function (commentsArray) {
    $.each(commentsArray, function (index, blogArticleComment) {
        $("#commentsSection").append(
            '<div class="jqm-comment media my-3" id="' + blogArticleComment.commentId + '" data-timestamp="' + blogArticleComment.timestamp + '">' +
                '<div class="jqm-comment-user-icon">' +
                    '<img class="img-fluid" src="' + blogArticleComment.authorImageUrl + '">' +
                '</div>' +
                '<div class="jqm-comment media-body px-3">' +
                    '<b>' + blogArticleComment.authorName + '</b> ' + (blogArticleComment.isMine ? '(you)' : '') + '<br/>' +
                    blogArticleComment.htmlContent +
                '</div>' +
            '</div>');
    });
};





