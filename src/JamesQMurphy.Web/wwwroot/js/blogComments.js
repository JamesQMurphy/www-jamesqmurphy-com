const COMMENTS_SECTION_ID = "commentsRoot";
const VIEW_MORE_CTL_SUFFIX = "_viewMoreCtl";

$(function () {
    var commentsSection = $("#" + COMMENTS_SECTION_ID);
    commentsSection.append($(BlogComments.HtmlForMoreBlock(COMMENTS_SECTION_ID, 'SHOW MORE COMMENTS')));
    BlogComments.ViewMoreCtl_BindClick(COMMENTS_SECTION_ID);

    var mutationObserver = new MutationObserver(BlogComments.OnDOMChange);
    mutationObserver.observe(commentsSection.get(0), { attributes: false, childList: true, characterData: false });

    // Preload a bunch of comments up-front
    BlogComments.FetchLatestComments();

    // Show the top-level comments
    setTimeout(function () {
        BlogComments.GetChildCommentElements(COMMENTS_SECTION_ID).show();
        BlogComments.ViewMoreCtl_Refresh(COMMENTS_SECTION_ID);
    }, 500);
});

function BlogComments() { }


BlogComments.lastTimestampRetrieved = '';

BlogComments.FetchLatestComments = function () {
    $.getJSON(window.location.href.split('#')[0] + '/comments?sinceTimestamp=' + BlogComments.lastTimestampRetrieved, function (commentsArray) {
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

BlogComments.GetChildCommentElements = function (commentId) {
    if (commentId === COMMENTS_SECTION_ID) {
        return $("#" + COMMENTS_SECTION_ID).children(".jqm-comment");
    }
    else {
        return $("#" + commentId + " > .jqm-comment-body").children(".jqm-comment");
    }
};

BlogComments.InsertCommentsIntoDOM = function (commentsArray) {
    $.each(commentsArray, function (_index, blogArticleComment) {

        // Figure out where to insert the new comment
        var parentId = blogArticleComment.replyToId || COMMENTS_SECTION_ID;
        var insertBeforeElement = $("#" + parentId + " [id][data-timestamp]").filter(function () {
            return ($(this).data('timestamp') > blogArticleComment.timestamp)
                && ($(this).attr('id').startsWith(parentId + '/'));
        }).first();

        // Insert it
        var newNode = $('<div class="jqm-comment media my-3" id="' + blogArticleComment.commentId + '" data-timestamp="' + blogArticleComment.timestamp + '">' +
            '<div class="jqm-comment-user-icon">' +
                '<img class="img-fluid" src="' + blogArticleComment.authorImageUrl + '">' +
            '</div>' +
            '<div class="jqm-comment-body media-body px-3">' +
                '<b>' + blogArticleComment.authorName + '</b> ' + (blogArticleComment.isMine ? '(you)' : '') + '<br/>' +
                blogArticleComment.htmlContent +
                BlogComments.HtmlForMoreBlock(blogArticleComment.commentId, 'VIEW REPLIES') +
            '</div>' + 
            '</div>').hide().insertBefore(insertBeforeElement);

        // Bind click event to control
        BlogComments.ViewMoreCtl_BindClick(blogArticleComment.commentId);

        // Set up mutation observer under new node's media-body
        var newNodeMediaBody = $(newNode).children('.media-body').get(0);
        var mutationObserver = new MutationObserver(BlogComments.OnDOMChange);
        mutationObserver.observe(newNodeMediaBody, { attributes: false, childList: true, characterData: false });
    });
};

BlogComments.HtmlForMoreBlock = function (id, viewText) {
    return '<div class="media my-3" id="' + id + '/more" data-timestamp="z">' +
        BlogComments.ViewMoreCtl_GenerateHtml(id, viewText) +
        '&nbsp;</div>';
};


BlogComments.ViewMoreCtl_GenerateHtml = function (id, innerText) {
    return '<span class="jqm-linkButton" id="' + id + VIEW_MORE_CTL_SUFFIX + '" style="display:none">' + innerText + '</span>';
};

BlogComments.ViewMoreCtl_Refresh = function (commentId) {
    var childComments = BlogComments.GetChildCommentElements(commentId);
    var hiddenCount = childComments.filter(":hidden").length;
   //var visibleCount = childComments.length - hiddenCount;
    if (hiddenCount > 0) {
        $("#" + commentId + VIEW_MORE_CTL_SUFFIX).show();
    }
    else {
        $("#" + commentId + VIEW_MORE_CTL_SUFFIX).hide();
    }
};

BlogComments.ViewMoreCtl_OnClick = function (event) {
    var commentId = event.target.id.replace(VIEW_MORE_CTL_SUFFIX, '');
    BlogComments.GetChildCommentElements(commentId).show();
    BlogComments.ViewMoreCtl_Refresh(commentId);
};

BlogComments.ViewMoreCtl_BindClick = function (commentId) {
    // Can't simply use $().click()
    // See https://makeitspendit.com/fix-jquery-click-event-not-working-with-dynamically-added-elements/
    $('body').on('click', "#" + commentId + VIEW_MORE_CTL_SUFFIX, BlogComments.ViewMoreCtl_OnClick);
};

BlogComments.OnDOMChange = function (mutations) {
    var insertions = 0;
    $.each(mutations, function (_index, mutationRecord) {
        var parentCommentId = mutationRecord.target.parentNode.getAttribute('id') || COMMENTS_SECTION_ID;
        $(mutationRecord.addedNodes).each(function (_index, addedNode) {
            var commentId = addedNode.getAttribute('id');
            BlogComments.ViewMoreCtl_Refresh(commentId);
            insertions++;
        });
        BlogComments.ViewMoreCtl_Refresh(parentCommentId);
    });
    console.log(insertions + " comment(s) inserted into DOM");
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


