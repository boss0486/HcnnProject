$(function () {

    // Reference the auto-generated proxy for the hub.
    var chat = $.connection.chatHub;

    // Create a function that the hub can call back to display messages.
    chat.client.addNewMessageToPage = function (name, message) {

        // Add the message to the page.
        var currentDate = new Date();
        var dateCreate = dateToFromNowDaily(currentDate);
        var timeCreate = timeformat(currentDate);

        if ($('#displayname').val() !== name) {
            $('#discussion').append(
                '<li class="clearfix"><div class="message-data text-left"><span class="message-data-time"><span class="message-data-time">' + dateCreate + ' '+ timeCreate +'</span> <strong>' +
                htmlEncode(name) +
                '</strong></span></div><div class="message other-message float-left"> ' +
                htmlEncode(message) +
                ' </div></li>');
        } else {
            $('#discussion').append('<li class="clearfix"><div class="message-data text-right"><span class="message-data-time">' + dateCreate + ' ' + timeCreate +'</span> <span class="message-data-time"><strong>' + htmlEncode(name) + '</strong></span></div><div class="message other-message float-right"> ' + htmlEncode(message) + ' </div></li>');
        }
    };
    // Get the user name and store it to prepend to messages.
    $('#displayname').val(prompt('Enter your name:', ''));
    loadChatLog();

    // Set initial focus to message input box.
    $('#message').focus();
    // Start the connection.
    $.connection.hub.start().done(function () {
        $('#sendmessage').click(function () {
            // Call the Send method on the hub.
            //                    chat.server.send($('#displayname').val(), $('#message').val());
            chat.server.send($('#displayname').val(), $('#message').val());
            // Clear text box and reset focus for next comment.
            $('#message').val('').focus();
        });
    });
});
// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

// call this function, passing-in your date
function dateToFromNowDaily(myDate) {

    // get from-now for this date
    var fromNow = moment(myDate).fromNow();

    // ensure the date is displayed with today and yesterday
    return moment(myDate).calendar(null, {
        // when the date is closer, specify custom values
        lastWeek: '[Last] dddd',
        lastDay: '[Yesterday]',
        sameDay: '[Today]',
        nextDay: '[Tomorrow]',
        nextWeek: 'dddd',
        // when the date is further away, use from-now functionality             
        sameElse: function () {
            return "[" + fromNow + "]";
        }
    });
}

function timeformat(date) {
    var h = date.getHours();
    var m = date.getMinutes();
    var x = h >= 12 ? 'pm' : 'am';
    h = h % 12;
    h = h ? h : 12;
    m = m < 10 ? '0' + m : m;
    var mytime = h + ':' + m + ' ' + x;
    return mytime;
}

function loadChatLog() {

    var jsonChatlog = GetChatLogData();

    var lstLog = JSON.parse(jsonChatlog);
    for (var i = 0; i < lstLog.length; i++) {
        $('#discussion').append('<li class="clearfix">------------------------' + lstLog[i].CreatedDate + '-------------------------</li>');

        for (var j = 0; j < lstLog[i].Item.length; j++) {

            var logItem = lstLog[i].Item[j];

            if ($('#displayname').val() !== logItem.CreatedBy) {
                $('#discussion').append(
                    '<li class="clearfix"><div class="message-data text-left"><span class="message-data-time"><span class="message-data-time">' + lstLog[i].CreatedDate + '</span> <strong>' +
                    htmlEncode(logItem.CreatedBy) +
                    '</strong></span></div><div class="message other-message float-left"> ' +
                    htmlEncode(logItem.Message) +
                    ' </div></li>');
            } else {
                $('#discussion').append('<li class="clearfix">' +
                    '<div class="message-data text-right">' +
                        '<span class="message-data-time">' + lstLog[i].CreatedDate + '</span> ' +
                            '<span class="message-data-time">' +
                    '           <strong>' + htmlEncode(logItem.CreatedBy) + '</strong>' +
                    '       </span>' +
                    '</div>' +
                    '<div class="message other-message float-right"> ' + htmlEncode(logItem.Message) + ' </div></li>');
            }
        }
    }
}


function GetChatLogData() {
    
//    var params = {
//        "usersJson": usersJson
//    };
    var message = "";
    $.ajax({
        type: "GET",
        traditional: true,
        async: false,
        cache: false,
        url: '/chat/GetDataChatLog',
//        data: getReportColumnsParams,
        success: function(result) {
            message = result.data;
        },
        error: function(xhr) {
            console.log(xhr.responseText);
            alert("Error has occurred..");
        }
    });
    return message;
} 