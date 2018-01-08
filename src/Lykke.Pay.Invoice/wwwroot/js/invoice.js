$.setUpCountDown = function () {
    $('#my-item').svgPie({ easing: 'swing', duration: $.refreshSeconds * 1000, percentage: 100, dimension: 30 });
    $.passedSeconds = 0;
    $.remainTimeInterval = setInterval(function() {
            $.passedSeconds++;
            $.updateProgress();
        },
        1000);
};

$.setUpUpdateStatus = function() {
    $.statusTimes = setInterval(function() {
            $.updateStatus();
        },
        15000);
};

$.updateStatus = function () {
    $.post($.updateStatusUrl,
        {
            'invoiceId': $.invoiceId
        },
        function (data) {
            console.log(data.status);
            if (data && $.invoiceStatus !== data.status) {
                window.location.href = window.location.href;
            }
        }
    );
};

$.updateProgress = function () {
    var remainSeconds = $.refreshSeconds - $.passedSeconds;
    var remainMinutes = remainSeconds / 60 | 0;

    if (remainSeconds < 1) {
        clearInterval($.remainTimeInterval);
        window.location.href = window.location.href;
        return;
    }

    if (remainMinutes > 1) {
        $('.invoice__remain-text').text('' + remainMinutes + ' min remaining…');
        $('.invoice__value').html('The address will be invalid in ' + remainMinutes + ' minutes<br> due to inactivity');
    } else if (remainMinutes === 1) {
        $('.invoice__remain-text').text('' + remainMinutes + ' min remaining…');
        $('.invoice__value').html('The address will be invalid in ' + remainMinutes + ' minute<br> due to inactivity');
    } else {
        $('.invoice__remain-text').text('< 1 min remaining…');
        $('.invoice__value').html('The address will be invalid in < 1 minute<br> due to inactivity');
    }
};
