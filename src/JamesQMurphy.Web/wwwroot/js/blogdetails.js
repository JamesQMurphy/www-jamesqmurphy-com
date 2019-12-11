$(function () {
    getComments();
});

function getComments() {
    $.getJSON(document.URL + '/comments', function (comments) {
        $.each(comments, function (index, value) {
            $("#commentsSection").append(
                '<div class="jqm-comment media my-3" id="' + value.timestamp + '">' +
                    '<div class="jqm-comment-user-icon">' +
                        '<img class="img-fluid" src="' + value.authorImageUrl + '">' +
                    '</div>' +
                    '<div class="jqm-comment media-body px-3">' +
                        '<b>' + value.authorName + '</b><br/>' +
                        value.htmlContent +
                    '</div>' +
                '</div>');
        });
    });
}