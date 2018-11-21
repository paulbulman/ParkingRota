document.addEventListener('DOMContentLoaded',
    function () {
        $("#selectAll").click(function () {
            $("input:checkbox").each(function () {
                this.checked = true;
            });
        });

        $("#selectNone").click(function () {
            $("input:checkbox").each(function () {
                this.checked = false;
            });
        });
    });