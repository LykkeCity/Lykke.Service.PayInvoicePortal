$.updateOrder = function () {
    var now = Date.now();
    if ($.invoiceTimeDueDate < now) {
        clearInterval($.intTimer);
        $('.invoice__remain-text').text('invoice is not avaible…');
        $('.invoice__value').html('The invoice is not avaible more');
        return;
    }

    $.post($.updateOrderUrl,
        {
            'invoiceId': $.invoiceId,
            'orderRequestId': $.orderRequestId
        },
        function (data) {
            if (!data) {
                $('.invoice__remain-text').text('invoice is not avaible…');
                $('.invoice__value').html('The invoice is not avaible more');
                return;
            }

            $('.invoice__qr > img').attr('src', data.qrCode);
            $('.invoice__payment').html('' + data.amount + ' BTC<div class="invoice__info"> for payment </div>');
            $('.invoice__original').html('' + data.origAmount + ' ' + data.currency + '<div class="invoice__info"> original </div>');
        }
    );
};

$.updateProgress = function () {
    var int = ($.invoiceTimeRefresh * 60) / 16;
    var index = $.imageIndex / int | 0;



    var minRemind = ($.invoiceTimeRefresh * 60 - $.imageIndex) / 60 | 0;
    if (index > 15) {
        $.updateOrder();
        $.imageIndex = 0;
        return;
    }
    var value = '0px -' + index * 35 + 'px';
    $('.invoice__remain-loading').css({ 'background-position': value });
    if (minRemind > 1) {
        $('.invoice__remain-text').text('' + minRemind + ' min remaining…');
        $('.invoice__value').html('The address will be invalid in ' + minRemind + ' minutes<br> due to inactivity');
    }
    else if (minRemind == 1) {
        $('.invoice__remain-text').text('' + minRemind + ' min remaining…');
        $('.invoice__value').html('The address will be invalid in ' + minRemind + ' minute<br> due to inactivity');
    } else {
        $('.invoice__remain-text').text('< 1 min remaining…');
        $('.invoice__value').html('The address will be invalid in < 1 minute<br> due to inactivity');
    }



}

$.startCountDown = function () {

    $.imageIndex = 0;
    $.intTimer = setInterval(function () {
        $.imageIndex++;
        $.updateProgress();
    }, 1000);
};


$(function () {
    var now = new Date;
    now.setMinutes(now.getMinutes() + $.invoiceTimeRefresh + 10);
    $.invoiceTimeRefresh = $.invoiceTimeRefresh || 1;
    $.invoiceTimeDueDate = $.invoiceTimeDueDate || now.getTime();
    $.startCountDown();

})