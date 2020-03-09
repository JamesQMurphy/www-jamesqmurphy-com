const COMMENTS_SECTION_ID = "commentsRoot";
const VIEW_MORE_CTL_SUFFIX = "_viewMoreCtl";
const REPLY_CTL_SUFFIX = "_replyCtl";
const HIDE_CTL_SUFFIX = "_hideCtl";
const DELETE_CTL_SUFFIX = "_deleteCtl";
const SUBMITREPLY_CTL_SUFFIX = "_submitReplyCtl";
const CANCELREPLY_CTL_SUFFIX = "_cancelReplyCtl";

// These are in milliseconds
const FETCHDELAY_ONVISIBLE = 3000;
const FETCHDELAY_JUSTRETRIEVED = 400;
const FETCHDELAY_JUSTPOSTED = 400;
const FETCHDELAY_NONEFOUND = 300000;

$(function () {
    var commentsSection = $("#" + COMMENTS_SECTION_ID);
    BlogComments.commentsUrl = window.location.href.split('#')[0] + '/comments';
    BlogComments.canComment = $("#submitUserComment").length > 0;

    // Determine the names of the hidden property and visibilityChangeEvent
    // Source: https://developer.mozilla.org/en-US/docs/Web/API/Page_Visibility_API
    if (typeof document.hidden !== "undefined") {
        BlogComments.hidden = "hidden";
        BlogComments.visibilityChange = "visibilitychange";
    } else if (typeof document.msHidden !== "undefined") {
        BlogComments.hidden = "msHidden";
        BlogComments.visibilityChange = "msvisibilitychange";
    } else if (typeof document.webkitHidden !== "undefined") {
        BlogComments.hidden = "webkitHidden";
        BlogComments.visibilityChange = "webkitvisibilitychange";
    }

    // Bind the "View More Comments" control
    BlogComments.ViewMoreCtl_BindClick(COMMENTS_SECTION_ID);

    // Wire up the OnDOM change event
    var mutationObserver = new MutationObserver(BlogComments.OnDOMChange);
    mutationObserver.observe(commentsSection.get(0), { attributes: false, childList: true, characterData: false });

    // Wire up click event for main comment submission
    $("#submitUserComment").click(function () {
        BlogComments.SubmitComments($("#userComment").val(), COMMENTS_SECTION_ID);
    });

    // Wire up the visibility event
    document.addEventListener(BlogComments.visibilityChange, function () {
        setTimeout(BlogComments.FetchLatestComments, FETCHDELAY_ONVISIBLE);
    }, false);

    // Preload a bunch of comments up-front
    BlogComments.FetchLatestComments();

    // Show the top-level comments
    setTimeout(function () {
        BlogComments.GetChildCommentElements(COMMENTS_SECTION_ID).show();
        BlogComments.ViewMoreCtl_Refresh(COMMENTS_SECTION_ID);
    }, FETCHDELAY_JUSTRETRIEVED);
});

function BlogComments() { }


BlogComments.lastTimestampRetrieved = '';
BlogComments.commentsUrl = '';
BlogComments.canComment = false;
BlogComments.hidden = "hidden";
BlogComments.visibilityChange = "visibilitychange";

BlogComments.FetchLatestComments = function () {
    if (!document[BlogComments.hidden]) {
        $.getJSON(BlogComments.commentsUrl + '?sinceTimestamp=' + BlogComments.lastTimestampRetrieved, function (commentsArray) {
            if (commentsArray.length > 0) {
                BlogComments.InsertReactionsIntoDOM(commentsArray);
                BlogComments.lastTimestampRetrieved = commentsArray[commentsArray.length - 1].timestamp;
                setTimeout(BlogComments.FetchLatestComments, FETCHDELAY_JUSTRETRIEVED);
            }
            else {
                setTimeout(BlogComments.FetchLatestComments, FETCHDELAY_NONEFOUND);
            }
        });
    }
};

BlogComments.SubmitComments = function (comment, replyToCommentId) {
    if (comment.length > 2) {
        var data = addAntiForgeryToken({
            userComment: comment,
            sinceTimestamp: BlogComments.lastTimestampRetrieved,
            replyTo: replyToCommentId === COMMENTS_SECTION_ID ? "" : replyToCommentId
        });
        $.post(BlogComments.commentsUrl, data)
            .done(function (pendingComment) {
                $('form').trigger('reset');
                BlogComments.InsertReactionsIntoDOM([pendingComment]);

                // Show the pending comment
                $('#' + pendingComment.commentId).show();

                // Set up to fetch more
                setTimeout(BlogComments.FetchLatestComments, FETCHDELAY_JUSTPOSTED);
            })
            .fail(function () { console.log('TODO: this failed'); });
        return true;
    }
    else {
        $('#CommentTooSmall').modal();
        return false;
    }
};

BlogComments.GetChildCommentElements = function (commentId) {
    if (commentId === COMMENTS_SECTION_ID) {
        return $("#" + COMMENTS_SECTION_ID).children(".jqm-comment");
    }
    else {
        return $("#" + commentId + " > .jqm-comment-body").children(".jqm-comment");
    }
};

BlogComments.InsertReactionsIntoDOM = function (reactionsArray) {
    $.each(reactionsArray, function (_index, blogArticleReaction) {

        // Figure out where to insert or replace the new comment
        var parentId = blogArticleReaction.replyToId || COMMENTS_SECTION_ID;

        // Generate new node
        var map = new Map();
        map.set('VIEW_MORE_CTL_SUFFIX', VIEW_MORE_CTL_SUFFIX);
        map.set('commentId',      blogArticleReaction.commentId);
        map.set('timestamp',      blogArticleReaction.timestamp);
        map.set('timeAgo',        moment(blogArticleReaction.timestamp).fromNow());
        map.set('authorName',     blogArticleReaction.authorName);
        map.set('authorImageUrl', blogArticleReaction.authorImageUrl);
        map.set('you',            blogArticleReaction.isMine ? '(you)' : '');
        map.set('editState',      blogArticleReaction.editState ? '(' + blogArticleReaction.editState + ')' : '');
        map.set('replyButton',    blogArticleReaction.canReply ? BlogComments.ReplyCtl_GenerateHtml(blogArticleReaction.commentId, 'Reply') : "");
        map.set('trashButton',    blogArticleReaction.canDelete ? BlogComments.DeleteCtl_GenerateHtml(blogArticleReaction.commentId) : "");
        map.set('htmlContent', blogArticleReaction.htmlContent);
        var newNode = $(ReplaceInTemplate("template#commentTemplate", map));

        // Insert or replace
        var existingElement = $("#" + blogArticleReaction.commentId);
        if (existingElement.length > 0) {
            if (existingElement.is(':hidden')) {
                newNode.hide();
            }
            existingElement.replaceWith(newNode);
        }
        else {
            // Insert new node (hidden)
            var insertBeforeElement = $("#" + parentId + " [id][data-timestamp]").filter(function () {
                return ($(this).data('timestamp') > blogArticleReaction.timestamp)
                    && ($(this).attr('id').startsWith(parentId + '/'));
            }).first();
            newNode.hide().insertBefore(insertBeforeElement);

            // Bind click events for controls
            BlogComments.ViewMoreCtl_BindClick(blogArticleReaction.commentId);
            BlogComments.ReplyCtl_BindClick(blogArticleReaction.commentId);
            BlogComments.DeleteCtl_BindClick(blogArticleReaction.commentId);

            // Set up mutation observer under new node's media-body
            var newNodeMediaBody = $(newNode).children('.media-body').get(0);
            var mutationObserver = new MutationObserver(BlogComments.OnDOMChange);
            mutationObserver.observe(newNodeMediaBody, { attributes: false, childList: true, characterData: false });
        }


    });
};



BlogComments.ViewMoreCtl_Refresh = function (commentId) {
    var childComments = BlogComments.GetChildCommentElements(commentId);
    var hiddenCount = childComments.filter(":hidden").length;
    if (hiddenCount > 0) {
        $("#" + commentId + VIEW_MORE_CTL_SUFFIX).show();
    }
    else {
        $("#" + commentId + VIEW_MORE_CTL_SUFFIX).hide();
    }
};

BlogComments.ViewMoreCtl_BindClick = function (commentId) {
    // Can't simply use $().click()
    // See https://makeitspendit.com/fix-jquery-click-event-not-working-with-dynamically-added-elements/
    $('body').on('click', "#" + commentId + VIEW_MORE_CTL_SUFFIX, BlogComments.ViewMoreCtl_OnClick);
};

BlogComments.ViewMoreCtl_OnClick = function (event) {
    var commentId = event.target.id.replace(VIEW_MORE_CTL_SUFFIX, '');
    BlogComments.GetChildCommentElements(commentId).show();
    BlogComments.ViewMoreCtl_Refresh(commentId);
};


BlogComments.ReplyCtl_GenerateHtml = function (commentId, innerText) {
    return '<span class="btn btn-sm btn-link p-0" id="' + commentId + REPLY_CTL_SUFFIX + '" >' + innerText + '</span>';
};

BlogComments.ReplyCtl_BindClick = function (commentId) {
    // Can't simply use $().click()
    // See https://makeitspendit.com/fix-jquery-click-event-not-working-with-dynamically-added-elements/
    $('body').on('click', "#" + commentId + REPLY_CTL_SUFFIX, BlogComments.ReplyCtl_OnClick);
};

BlogComments.ReplyCtl_OnClick = function (event) {
    var commentId = event.target.id.replace(REPLY_CTL_SUFFIX, '');
    var originalReplyButton = $(event.target);

    // Create or show reply form
    var replyForm = $("#" + commentId + "\\/reply");
    if (replyForm.length === 0) {
        // Create form
        var map = new Map();
        map.set('commentId', commentId);
        map.set('SUBMITREPLY_CTL_SUFFIX', SUBMITREPLY_CTL_SUFFIX);
        map.set('CANCELREPLY_CTL_SUFFIX', CANCELREPLY_CTL_SUFFIX);
        map.set('authorName', $("#" + commentId + "\\/authorName").text());
        replyForm = $(ReplaceInTemplate("template#replyToCommentTemplate", map)).insertAfter($("#" + commentId + " > .jqm-comment-body > .jqm-comment-controls").first());

        // Wire up form buttons
        $('body').on('click', "#" + commentId + SUBMITREPLY_CTL_SUFFIX, function (event) {
            if (BlogComments.SubmitComments($("#userComment" + commentId).val(), commentId)) {
                replyForm.hide();
                originalReplyButton.show();
            }
        });
        $('body').on('click', "#" + commentId + CANCELREPLY_CTL_SUFFIX, function (event) {
            replyForm.hide();
            originalReplyButton.show();
        });
    }
    replyForm.show();
    originalReplyButton.hide();
};


BlogComments.DeleteCtl_GenerateHtml = function (id) {
    return '<span class="fas fa-trash-alt p-0 text-muted" id="' + id + DELETE_CTL_SUFFIX + '" ></span>';
};

BlogComments.DeleteCtl_BindClick = function (commentId) {
    // Can't simply use $().click()
    // See https://makeitspendit.com/fix-jquery-click-event-not-working-with-dynamically-added-elements/
    $('body').on('click', "#" + commentId + DELETE_CTL_SUFFIX, BlogComments.DeleteCtl_OnClick);
};

BlogComments.DeleteCtl_OnClick = function (event) {
    var commentId = event.target.id.replace(DELETE_CTL_SUFFIX, '');
    alert('clicked Delete for ' + commentId);
};


BlogComments.OnDOMChange = function (mutations) {
    var insertions = 0;
    $.each(mutations, function (_index, mutationRecord) {
        var parentCommentId = mutationRecord.target.parentNode.getAttribute('id') || COMMENTS_SECTION_ID;
        $(mutationRecord.addedNodes).filter('.jqm-comment').each(function (_index, addedNode) {
            var commentId = addedNode.getAttribute('id');
            BlogComments.ViewMoreCtl_Refresh(commentId);
            insertions++;
        });
        BlogComments.ViewMoreCtl_Refresh(parentCommentId);
    });
};

function ReplaceInTemplate(templateSelector, mapValues) {
    var returnHtml = $(templateSelector).html();
    mapValues.forEach(function (value, key) {
        returnHtml = returnHtml.replace(new RegExp('\{' + key + '\}', 'g'), value);
    });
    return returnHtml;
}

// Polyfill for String.startsWith
if (!String.prototype.startsWith) {
    Object.defineProperty(String.prototype, 'startsWith', {
        value: function (search, rawPos) {
            var pos = rawPos > 0 ? rawPos | 0 : 0;
            return this.substring(pos, pos + search.length) === search;
        }
    });
}

// Polyfill for templates
// https://stackoverflow.com/a/33138997/1001100
function templateContent(template) {
    if ("content" in document.createElement("template")) {
        return document.importNode(template.content, true);
    } else {
        var fragment = document.createDocumentFragment();
        var children = template.childNodes;
        for (i = 0; i < children.length; i++) {
            fragment.appendChild(children[i].cloneNode(true));
        }
        return fragment;
    }
}
