

    $(function () {
        
        $("#alert").dialog({
            autoOpen: false,
            position: { my: "top", at: "top", of: window },
            minWidth: 400,
            show: {
                effect: "fold",
                duration: "slow"
            },
            hide: {
                effect: "fold",
                duration: "slow"
            }
        });

        if ($('#alert').children('div').length > 0) {

            $("#alert").dialog("open");
        }

        $("#ui-dialog-title-dialog").hide();
        $(".ui-dialog-titlebar").removeClass('ui-widget-header');
    });

