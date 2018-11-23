$(document).ready(function () {
    $("#qrCode").qrcode($("#qrCodeData").attr("data-url"));
});