$(function () {
    getLatestComments();
});

function getLatestComments() {
    $.getJSON(document.URL + '/comments?sinceTimestamp=' + getLatestComments.lastCommentRetrieved, function (commentsArray) {
        if (commentsArray.length > 0) {
            insertCommentsIntoDOM(commentsArray);
            getLatestComments.lastCommentRetrieved = commentsArray[commentsArray.length - 1].timestamp;
            setTimeout(getLatestComments, 15000);
        }
        else {
            setTimeout(getLatestComments, 15000);
        }
    });
}

getLatestComments.lastCommentRetrieved = '';

function insertCommentsIntoDOM(commentsArray) {
    $.each(commentsArray, function (index, blogArticleComment) {
        $("#commentsSection").append(
            '<div class="jqm-comment media my-3" id="' + blogArticleComment.commentId + '">' +
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