const COMMENTS_SECTION_ID = "commentsSection";

$(function () {
    var commentsSection = $("#" + COMMENTS_SECTION_ID);
    commentsSection.append(BlogComments.HtmlForMoreBlock('', 'SHOW MORE COMMENTS'));

    var mutationObserver = new MutationObserver(BlogComments.OnDOMChange);
    mutationObserver.observe(commentsSection.get(0), { attributes: false, childList: true, characterData: false });

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
    console.log("inside BlogComments.InsertCommentsIntoDOM, commentsArray.length = " + commentsArray.length);
    $.each(commentsArray, function (_index, blogArticleComment) {

        // Figure out where to insert the new comment
        var parentId = blogArticleComment.replyToId || COMMENTS_SECTION_ID;
        var insertBeforeElement = $("#" + parentId + " [id][data-timestamp]").filter(function () {
            return ($(this).data('timestamp') > blogArticleComment.timestamp)
                && ($(this).attr('id').startsWith(blogArticleComment.replyToId + '/'));
        }).first();

        // Insert it
        var newNode = $('<div class="jqm-comment media my-3" id="' + blogArticleComment.commentId + '" data-timestamp="' + blogArticleComment.timestamp + '">' +
            '<div class="jqm-comment-user-icon">' +
                '<img class="img-fluid" src="' + blogArticleComment.authorImageUrl + '">' +
            '</div>' +
            '<div class="jqm-comment media-body px-3">' +
                '<b>' + blogArticleComment.authorName + '</b> ' + (blogArticleComment.isMine ? '(you)' : '') + '<br/>' +
                blogArticleComment.htmlContent +
                BlogComments.HtmlForMoreBlock(blogArticleComment.commentId, 'VIEW REPLIES') +
            '</div>' + 
            '</div>').insertBefore(insertBeforeElement);

        // Set up mutation observer under new node's media-body
        var newNodeMediaBody = $(newNode).children('.media-body').get(0);
        var mutationObserver = new MutationObserver(BlogComments.OnDOMChange);
        mutationObserver.observe(newNodeMediaBody, { attributes: false, childList: true, characterData: false });
    });
};

BlogComments.HtmlForMoreBlock = function (id, viewText) {
    return '<div class="media my-3" id="' + id + '/more" data-timestamp="z">' +
        '<span class="jqm-viewMoreControl">' + viewText + '</span>&nbsp;</div>';
};

BlogComments.OnDOMChange = function (mutations) {
    $.each(mutations, function (_index, mutationRecord) {
        var commentId = mutationRecord.target.parentNode.getAttribute('id') || COMMENTS_SECTION_ID;
        if (commentId === COMMENTS_SECTION_ID) {
            console.log('New comment');
        }
        else {
            console.log('New reply to comment ' + commentId);
        }
        $(mutationRecord.addedNodes).each(function (_index, addedNode) {
            console.log("Added node: " + addedNode.getAttribute('id'));
        });

        var showMoreControls = $("#" + commentId + " .jqm-viewMoreControl");
        var hiddenComments = $(mutationRecord.target).children('.jqm-comment:hidden');
        if (hiddenComments.length > 0) {
            showMoreControls.show();
        }
        else {
            showMoreControls.hide();
        }
    });
};

// Polyfill for String.startsWith
if (!String.prototype.startsWith) {
    Object.defineProperty(String.prototype, 'startsWith', {
        value: function (search, rawPos) {
            var pos = rawPos > 0 ? rawPos | 0 : 0;
            return this.substring(pos, pos + search.length) === search;
        }
    });
}


