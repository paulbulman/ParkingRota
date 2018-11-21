$(document).ready(function () {
    $("#SelectedUserId").change(function () {
        window.location.href = '/OverrideRequests/' + $("#SelectedUserId option:selected").val();
    });
});