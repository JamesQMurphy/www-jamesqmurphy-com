$(function () {
    getComments();
});

function getComments() {
    $.getJSON(document.URL + '/comments', function (comments) {
        $.each(comments, function (index, value) {
            $("#commentsSection").append(
                '<div class="media my-3" id="' +
                value.timestamp +
                '"><img src="/images/unknownPersonPlaceholder.png"><div class="media-body px-3"><b>' +
                value.authorName +
                '</b><br/>'
                + value.htmlContent +
                "</div></div>");
        });
    });
}