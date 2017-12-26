$.updateStatus = function() {
    $.post($.updateStatusUrl,
        {
            'address': $.walletAddress
        },
        function (data) {

            if (data) {
                if ($.invoiceStatus != data.status) {
                    window.location.href = window.location.href;
                    return;
                }
            }

        }
    );
}

$.updateOrder = function () {
    //var now = Date.now();
    //if ($.invoiceTimeDueDate < now) {
    //    clearInterval($.intTimer);
    //    $('.invoice__remain-text').text('invoice is not avaible…');
    //    $('.invoice__value').html('The invoice is not avaible more');
    //    return;
    //}

    $.post($.updateOrderUrl,
        {
            'invoiceId': $.invoiceId,
            'address': $.walletAddress
        },
        function (data) {
            
            if (!data) {
                $('.invoice__remain-text').text('invoice is not avaible…');
                $('.invoice__value').html('The invoice is not avaible more');
                return;
            }

            $.invoiceTimeRefresh = data.invoiceTimeRefresh;
            $.startCountDown();

            $('.invoice__qr > img').attr('src', data.order.qrCode);
            $('.invoice__payment').html('' + data.order.amount + ' BTC<div class="invoice__info"> for payment </div>');
            $('.invoice__original').html('' + data.order.origAmount + ' ' + data.currency + '<div class="invoice__info"> original </div>');

            if ($.invoiceStatus != data.status) {
                window.location.href = window.location.href;
                return;
            }

            $.needAutoUpdate = data.needAutoUpdate;


        }
    );
};

$.updateProgress = function () {
    

    var secRemind = $.invoiceTimeRefresh - $.imageIndex;
    var minRemind = secRemind / 60 | 0;
    if (secRemind < 1) {
        clearInterval($.intTimer);
        $.updateOrder();
        $.imageIndex = 0;
        $('.invoice__remain-text').text('…');
        $('.invoice__value').html('…');
        return;
    }


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
    
    $.invoiceTimeRefresh = $.invoiceTimeRefresh || 1 * 60;
    $.invoiceTimeDueDate = $.invoiceTimeDueDate || 60 * 60;
    if ($.invoiceTimeRefresh > $.invoiceTimeDueDate) {
        $.invoiceTimeRefresh = $.invoiceTimeDueDate;
    }
    $.startCountDown();

    var statusTimes = setInterval(function () {
        $.imageIndex++;
        $.updateStatus();
    }, 15000);

    //var animationData = { "v": "4.6.0", "fr": 60, "ip": 0, "op": 81, "w": 200, "h": 200, "nm": "loader-color@3x", "ddd": 0, "assets": [], "layers": [{ "ddd": 0, "ind": 1, "ty": 4, "nm": "Shape Layer 2", "ks": { "o": { "a": 0, "k": 100 }, "r": { "a": 0, "k": 0 }, "p": { "a": 0, "k": [100, 102.5, 0] }, "a": { "a": 0, "k": [18, -344, 0] }, "s": { "a": 0, "k": [100, 100, 100] } }, "ao": 0, "shapes": [{ "ty": "gr", "it": [{ "d": 1, "ty": "el", "s": { "a": 0, "k": [180, 180] }, "p": { "a": 0, "k": [0, 0] }, "nm": "Ellipse Path 1", "mn": "ADBE Vector Shape - Ellipse" }, { "ty": "st", "c": { "a": 1, "k": [{ "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 0, "s": [0.6705883, 0, 1, 1], "e": [0.6705883, 0, 1, 1] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 20, "s": [0.6705883, 0, 1, 1], "e": [1, 0.5686275, 0, 1] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 40, "s": [1, 0.5686275, 0, 1], "e": [1, 0, 0.1607843, 1] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 60, "s": [1, 0, 0.1607843, 1], "e": [0.6705883, 0, 1, 1] }, { "t": 80 }] }, "o": { "a": 0, "k": 100 }, "w": { "a": 0, "k": 12 }, "lc": 2, "lj": 1, "ml": 4, "nm": "Stroke 1", "mn": "ADBE Vector Graphic - Stroke" }, { "ty": "tr", "p": { "a": 0, "k": [18.137, -347.113], "ix": 2 }, "a": { "a": 0, "k": [0, 0], "ix": 1 }, "s": { "a": 0, "k": [100, 100], "ix": 3 }, "r": { "a": 0, "k": 0, "ix": 6 }, "o": { "a": 0, "k": 100, "ix": 7 }, "sk": { "a": 0, "k": 0, "ix": 4 }, "sa": { "a": 0, "k": 0, "ix": 5 }, "nm": "Transform" }], "nm": "Ellipse 1", "np": 3, "mn": "ADBE Vector Group" }, { "ty": "tm", "s": { "a": 1, "k": [{ "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 0, "s": [0], "e": [0] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 20, "s": [0], "e": [74] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 41, "s": [74], "e": [1] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 42, "s": [1], "e": [75] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 60, "s": [75], "e": [75] }, { "t": 80 }], "ix": 1 }, "e": { "a": 1, "k": [{ "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 0, "s": [1], "e": [75] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 20, "s": [75], "e": [75] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 41, "s": [75], "e": [0] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 42, "s": [0], "e": [0] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 60, "s": [0], "e": [74] }, { "t": 80 }], "ix": 2 }, "o": { "a": 1, "k": [{ "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 0, "s": [90], "e": [270] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 20, "s": [270], "e": [360] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 41, "s": [360], "e": [270] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 42, "s": [270], "e": [450] }, { "i": { "x": [0.833], "y": [0.833] }, "o": { "x": [0.167], "y": [0.167] }, "n": ["0p833_0p833_0p167_0p167"], "t": 60, "s": [450], "e": [540] }, { "t": 80 }], "ix": 3 }, "m": 1, "ix": 2, "nm": "Trim Paths 1", "mn": "ADBE Vector Filter - Trim" }], "ip": 0, "op": 120, "st": 0, "bm": 0, "sr": 1 }] };
    //var params = {
    //    container: document.getElementById('bodymovin'),
    //    renderer: 'svg',
    //    loop: true,
    //    autoplay: true,
    //    animationData: animationData
    //};

    //var anim;

    //anim = bodymovin.loadAnimation(params);

})