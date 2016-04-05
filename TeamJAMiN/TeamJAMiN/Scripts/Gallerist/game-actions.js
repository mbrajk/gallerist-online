﻿//JS file for signalR communication

$(function () {
    var actionManager = $.connection.gameActionHub;
    actionManager.client.serverResponse = function () {
        window.location.reload(true);
    };
    $.connection.hub.start().done(function () {
        var actionSpace = $('.action-space');
        actionSpace.hover(
            function () { $(this).addClass('clickable') },
            function () { $(this).removeClass('clickable') }
        );
        actionSpace.click(function () {
            actionManager.server.update($(this).data('location'));
        });
    });
});