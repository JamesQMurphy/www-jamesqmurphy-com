$(function () {
    getLatestComments();
});

function getLatestComments() {
    $.getJSON(document.URL + '/comments?sinceTimestamp=' + getLatestComments.lastTimestampRetrieved, function (commentsArray) {
        if (commentsArray.length > 0) {
            insertCommentsIntoDOM(commentsArray);
            getLatestComments.lastTimestampRetrieved = commentsArray[commentsArray.length - 1].timestamp;
            setTimeout(getLatestComments, 15000);
        }
        else {
            setTimeout(getLatestComments, 15000);
        }
    });
}

getLatestComments.lastTimestampRetrieved = '';

function insertCommentsIntoDOM(commentsArray) {
    $.each(commentsArray, function (index, blogArticleComment) {
        $("#commentsSection").append(
            '<div class="jqm-comment media my-3" id="' + blogArticleComment.commentId + '" data-timestamp="' + blogArticleComment.timestamp +'">' +
                '<div class="jqm-comment-user-icon">' +
                    '<img class="img-fluid" src="' + blogArticleComment.authorImageUrl + '">' +
                '</div>' +
                '<div class="jqm-comment media-body px-3">' +
                    '<b>' + blogArticleComment.authorName + '</b> ' + (blogArticleComment.isMine ? '(you)' : '') + '<br/>' +
                    blogArticleComment.htmlContent +
                '</div>' +
            '</div>');
    });
}