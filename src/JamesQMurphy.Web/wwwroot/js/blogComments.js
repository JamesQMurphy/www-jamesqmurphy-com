const COMMENTS_SECTION_ID = "commentsRoot";
const VIEW_MORE_CONTROL_SUFFIX = "_viewMoreCtl";

$(function () {
    var commentsSection = $("#" + COMMENTS_SECTION_ID);
    commentsSection.append(BlogComments.HtmlForMoreBlock(COMMENTS_SECTION_ID, 'SHOW MORE COMMENTS'));
    BlogComments.BindClickViewMoreControl(COMMENTS_SECTION_ID);

    var mutationObserver = new MutationObserver(BlogComments.OnDOMChange);
    mutationObserver.observe(commentsSection.get(0), { attributes: false, childList: true, characterData: false });

    // Preload and show a bunch up-front
    BlogComments.FetchLatestComments();
    setTimeout(function () {
        $(".jqm-comment").show();
        BlogComments.RefreshShowMoreControls(COMMENTS_SECTION_ID);
    }, 1000);
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

BlogComments.GetChildComments = function (commentId) {
    if (commentId === COMMENTS_SECTION_ID) {
        return $("#" + COMMENTS_SECTION_ID).children(".jqm-comment");
    }
    else {
        return $("#" + commentId + " > .jqm-comment-body").children(".jqm-comment");
    }
};

BlogComments.InsertCommentsIntoDOM = function (commentsArray) {
    console.log("inside BlogComments.InsertCommentsIntoDOM, commentsArray.length = " + commentsArray.length);
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

        // Set up mutation observer under new node's media-body
        var newNodeMediaBody = $(newNode).children('.media-body').get(0);
        var mutationObserver = new MutationObserver(BlogComments.OnDOMChange);
        mutationObserver.observe(newNodeMediaBody, { attributes: false, childList: true, characterData: false });
    });
};

BlogComments.RefreshShowMoreControls = function (commentId) {
    var showMoreDiv = $("#" + commentId + "\\/more");
    var showMoreControls = $(showMoreDiv).children(".jqm-viewMoreControl");
    //console.log("showMoreControls id: " + showMoreControls.get(0).getAttribute('id'));
    var hiddenComments = $(showMoreDiv).siblings('.jqm-comment:hidden');
    if (hiddenComments.length > 0) {
        showMoreControls.show();
    }
    else {
        showMoreControls.hide();
    }

};

BlogComments.HtmlForMoreBlock = function (id, viewText) {
    return '<div class="media my-3" id="' + id + '/more" data-timestamp="z">' +
        '<span class="jqm-viewMoreControl" id="' + id + VIEW_MORE_CONTROL_SUFFIX + '">' + viewText + '</span>&nbsp;</div>';
};

BlogComments.ViewMoreClick = function (event) {
    console.log("clicked: " + event.target.id);
    var commentId = event.target.id.replace(VIEW_MORE_CONTROL_SUFFIX, '');
    BlogComments.GetChildComments(commentId).show();
    BlogComments.RefreshShowMoreControls(commentId);
};

BlogComments.BindClickViewMoreControl = function (commentId) {
    // Can't simply use $().click()
    // See https://makeitspendit.com/fix-jquery-click-event-not-working-with-dynamically-added-elements/
    $('body').on('click', "#" + commentId + VIEW_MORE_CONTROL_SUFFIX, BlogComments.ViewMoreClick);
};

BlogComments.OnDOMChange = function (mutations) {
    $.each(mutations, function (_index, mutationRecord) {
        var parentCommentId = mutationRecord.target.parentNode.getAttribute('id') || COMMENTS_SECTION_ID;
        if (parentCommentId === COMMENTS_SECTION_ID) {
            console.log('New comment(s)');
        }
        else {
            console.log('New replies to comment ' + parentCommentId);
        }
        $(mutationRecord.addedNodes).each(function (_index, addedNode) {
            var commentId = addedNode.getAttribute('id');
            console.log("New comment id: " + commentId);
            BlogComments.RefreshShowMoreControls(commentId);
            BlogComments.BindClickViewMoreControl(commentId);
        });
        BlogComments.RefreshShowMoreControls(parentCommentId);
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


