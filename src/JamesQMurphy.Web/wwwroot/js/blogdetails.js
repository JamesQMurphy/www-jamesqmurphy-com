$(function () {
    getComments();
});

function getComments() {
    $.getJSON(document.URL + '/comments', function (comments) {
        $.each(comments, function (index, value) {
            $("#commentsSection").append('<div class="media border mb-3"><p class="p-3"><b>' + value.authorName + '</b></p><div class="media-body"><p class="p-3">' + value.content + "</p></div></div>");
        });
    });
}