// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Show cookie consent message if present
$('#cookieConsent').toast('show');

// Add AntiForgeryToken to Ajax POST calls
// See https://stackoverflow.com/a/4074289/1001100
addAntiForgeryToken = function (data) {
    data.__RequestVerificationToken = $('form input[name=__RequestVerificationToken]').val();
    return data;
};
