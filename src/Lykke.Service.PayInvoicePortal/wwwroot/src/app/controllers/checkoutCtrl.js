(function () {
    'use strict';

    angular
        .module('app')
        .controller('checkoutCtrl', checkoutCtrl);

    checkoutCtrl.$inject = ['$window', '$location', '$scope', '$log', '$interval', '$timeout', 'apiSvc', 'fileSvc', 'statusSvc'];

    function checkoutCtrl($window, $location, $scope, $log, $interval, $timeout, apiSvc, fileSvc, statusSvc) {
        var vm = this;

        vm.callback = {
            url: ''
        };

        vm.pie = {
            transform1: '',
            transform2: ''
        };

        vm.header = {
            title: '',
            messages: '',
            color: '',
            icon: ''
        };

        var extendedTotalSeconds = 0,
            extendedRemainingSeconds = 0;

        vm.timer = {
            interval: null,
            total: 0,
            seconds: 0,
            mins: 0,
            isExtended: false
        };

        vm.status = {
            timeout: null
        };

        var qrUrlBegin = 'https://chart.googleapis.com/chart?chs=';
        var qrUrlEnd = '&chld=L|0&cht=qr&chl=';

        vm.const = {
            qrUrlSize220: qrUrlBegin + '220x220' + qrUrlEnd,
            qrUrlSize152: qrUrlBegin + '152x152' + qrUrlEnd
        };

        vm.model = {
            id: '',
            number: '',
            merchantId: '',
            merchant: '',
            paymentAmount: 0,
            settlementAmount: 0,
            paymentAsset: '',
            paymentAssetDisplay: '',
            paymentAssetSelect: '',
            settlementAsset: '',
            settlementAssetDisplay: '',
            paymentAssetAccuracy: 0,
            settlementAssetAccuracy: 0,
            exchangeRate: {
                value: 0,
                hidden: false
            },
            dueDate: $window.moment(),
            paidAmount: 0,
            paidDate: null,
            note: '',
            qrCode: '',
            walletAddress: '',
            files: [],
            waiting: false,
            message: '',
            paymentAssets: []
        };

        vm.handlers = {
            init: init,
            getFileExtension: fileSvc.getExtension,
            getFileSize: fileSvc.getSize,
            getFile: getFile,
            refreshDetails: refreshDetails
        };

        activate();

        function activate() {
            vm.callback.url = getParameterByName('callback', $window.location.href);

            $scope.$on('$destroy',
                function() {
                    stopTimer();
                    stopStatusTimeout();
                });

            $scope.$watch(
                function() {
                    return vm.model.paymentAssetSelect;
                },
                function(newValue, oldValue) {
                    console.log('newValue, oldValue', newValue, oldValue);
                });
        }

        function init(data) {
            if (gotoCallbackUrl(data.status))
                return;
            apply(data);

            apiSvc
                .getPaymentAssets(
                    vm.model.merchantId,
                    vm.model.settlementAsset
                )
                .then(
                    function(data) {
                        vm.model.paymentAssets = data || [];
                    },
                    function(error) {
                        $log.error(error);
                    }
                );

            startStatusTimeout();
        }

        function apply(data) {
            vm.model.id = data.id;
            vm.model.number = data.number;
            vm.model.status = data.status;
            vm.model.merchantId = data.merchantId;
            vm.model.merchant = data.merchant;
            vm.model.paymentAmount = data.paymentAmount;
            vm.model.settlementAmount = data.settlementAmount;
            vm.model.paymentAsset = data.paymentAsset;
            vm.model.paymentAssetDisplay = data.paymentAsset;
            vm.model.paymentAssetSelect = data.paymentAsset;
            vm.model.settlementAsset = data.settlementAsset;
            vm.model.settlementAssetDisplay = data.settlementAssetDisplay;
            vm.model.paymentAssetAccuracy = data.paymentAssetAccuracy;
            vm.model.settlementAssetAccuracy = data.settlementAssetAccuracy;
            vm.model.exchangeRate.value = data.exchangeRate;
            vm.model.deltaSpread = data.deltaSpread;
            vm.model.percents = data.percents;
            vm.model.pips = data.pips;
            vm.model.fee = data.fee;
            vm.model.dueDate = $window.moment(data.dueDate);
            vm.model.paidAmount = data.paidAmount;
            vm.model.paidDate = data.paidDate ? $window.moment(data.paidDate) : null;
            vm.model.note = data.note;
            vm.model.walletAddress = data.walletAddress;
            vm.model.files = data.files;

            vm.model.exchangeRate.hidden = data.paymentAsset === data.settlementAsset;

            extendedTotalSeconds = data.extendedTotalSeconds;
            extendedRemainingSeconds = data.extendedRemainingSeconds;

            updateMessage(data);

            if (data.status === 'Unpaid') {
                // bip21 for BTC
                // https://github.com/bitcoin/bips/blob/master/bip-0021.mediawiki#examples
                var labelEncoded = encodeURIComponent('invoice #' + data.number);

                vm.model.qrCode = encodeURIComponent('bitcoin:' +
                    data.walletAddress +
                    '?amount=' +
                    data.paymentAmount +
                    '&label=' +
                    labelEncoded +
                    '&message=' +
                    data.paymentRequestId);

                if (data.remainingSeconds > 0) {
                    // timer before order.DueDate
                    vm.timer.total = data.totalSeconds;
                    vm.timer.seconds = data.remainingSeconds;
                    vm.timer.isExtended = false;
                } else {
                    // timer before order.ExtendedDueDate
                    initExtendedTimer();
                }

                vm.model.waiting = true;

                restartTimer();
            } else {
                vm.model.qrCode = '';
                vm.timer.total = 0;
                vm.timer.seconds = 0;
                vm.model.waiting = false;

                updateHeader();
            }
        }

        function initExtendedTimer() {
            vm.timer.total = extendedTotalSeconds;
            vm.timer.seconds = extendedRemainingSeconds;
            vm.timer.isExtended = true;
        }

        function restartTimer() {
            stopTimer();
            updatePie();
            vm.timer.interval = $interval(tick, 1000);
        }

        function updateHeader() {
            var paidAmountText =
                vm.model.paidAmount.toLocaleString(undefined,
                        { minimumFractionDigits: vm.model.paymentAssetAccuracy }) +
                    ' ' +
                    vm.model.paymentAsset;

            var dateText = vm.model.paidDate ? vm.model.paidDate.format('l') : '';
            var receivedDateText = ' received on ' + dateText;

            switch (vm.model.status) {
                case 'InProgress':
                    vm.header.title = 'InProgress';
                    vm.header.message = 'Payment in progress';
                    vm.header.icon = 'icon--check_circle';
                    vm.header.color = statusSvc.getAlertStatusCss(vm.model.status);
                    break;
                case 'Paid':
                    vm.header.title = 'Paid';
                    vm.header.message = 'Invoice has been paid on ' + dateText;
                    vm.header.icon = 'icon--check_circle';
                    vm.header.color = statusSvc.getAlertStatusCss(vm.model.status);
                    break;
                case 'RefundInProgress':
                    vm.header.title = 'InProgress';
                    vm.header.message = 'Refund in progress';
                    vm.header.icon = '';
                    vm.header.color = statusSvc.getAlertStatusCss(vm.model.status);
                    break;
                case 'Refunded':
                    vm.header.title = 'Refund';
                    vm.header.message = 'Invoice has been refunded on ' + dateText;
                    vm.header.icon = 'icon--refund';
                    vm.header.color = statusSvc.getAlertStatusCss(vm.model.status);
                    break;
                case 'Underpaid':
                    vm.header.title = 'Underpaid';
                    vm.header.message = paidAmountText + receivedDateText;
                    vm.header.icon = 'icon--remove_circle';
                    vm.header.color = statusSvc.getAlertStatusCss(vm.model.status);
                    break;
                case 'Overpaid':
                    vm.header.title = 'Overpaid';
                    vm.header.message = paidAmountText + receivedDateText;
                    vm.header.icon = 'icon--add_circle';
                    vm.header.color = statusSvc.getAlertStatusCss(vm.model.status);
                    break;
                case 'LatePaid':
                    vm.header.title = 'LatePaid';
                    vm.header.message = paidAmountText + receivedDateText;
                    vm.header.icon = 'icon--warning_icn';
                    vm.header.color = statusSvc.getAlertStatusCss(vm.model.status);
                    break;
                case 'NotConfirmed':
                    vm.header.title = 'Error';
                    vm.header.message = 'Transfer hasn\'t been confirmed';
                    vm.header.icon = 'icon--warning_icn';
                    vm.header.color = statusSvc.getAlertStatusCss(vm.model.status);
                    break;
                case 'InternalError':
                    vm.header.title = 'Error';
                    vm.header.message = 'Internal error occurred';
                    vm.header.icon = 'icon--warning_icn';
                    vm.header.color = statusSvc.getAlertStatusCss(vm.model.status);
                    break;
                default:
                    vm.header.title = 'Error';
                    vm.header.message = 'Unknown status';
                    vm.header.icon = 'icon--warning_icn';
                    vm.header.color = 'alert--red';
                    break;
            }
        }

        function updateMessage(data) {
            vm.model.message = '';

            var values = [];

            if (data.percents > 0)
                values.push(data.percents.toLocaleString(undefined, { minimumFractionDigits: 1 }) + '%');

            if (data.pips > 0)
                values.push(data.pips + ' pips');

            if(data.fee > 0)
                values.push(data.fee.toLocaleString(undefined, { minimumFractionDigits: data.settlementAssetAccuracy }) + ' ' + data.settlementAsset);

            var fee;

            if (values.length === 3) {
                fee = values[0] + ', ' + values[1] + ' and ' + values[2];
            } else {
                fee = values.join(' and ');
            }

            if (data.paymentAsset === data.settlementAsset) {
                if (data.deltaSpread && fee) {
                    vm.model.message = 'Includes ' + fee + ' fee of processing payment.';
                }
            } else {
                if (data.deltaSpread) {
                    if (data.percents > 0 && data.pips === 0 && data.fee === 0) {
                        vm.model.message = 'Includes ' + fee + ' for covering the exchange risk';
                    } else if (fee) {
                        vm.model.message = 'Includes ' + fee + ' uplift for covering the exchange risk and the fee of processing payment.';
                    }
                } else if (fee) {
                    vm.model.message = 'Includes ' + fee + ' fee of processing payment.';
                }
            }
        }

        function updateDetails() {
            apiSvc.getPaymentDetails(vm.model.id)
                .then(
                    function (data) {
                        if (!data) {
                            $window.location.href = $window.location.href;
                        }

                        apply(data);
                    },
                    function (error) {
                        $log.error(error);
                    });
        }

        function updateStatus() {
            apiSvc.getPaymentStatus(vm.model.id)
                .then(
                    function (data) {
                        if (data.status !== vm.model.status) {
                            stopTimer();

                            if (!gotoCallbackUrl(data.status)) {
                                updateDetails();
                            }
                        }
                        startStatusTimeout();
                    },
                    function (error) {
                        $log.error(error);
                        startStatusTimeout();
                    });
        }

        function tick() {
            vm.timer.seconds--;
            vm.timer.mins = (vm.timer.seconds / 60 | 0) + 1;

            if (vm.timer.seconds <= 0) {
                if (!vm.timer.isExtended) {
                    initExtendedTimer();
                    restartTimer();
                } else {
                    refreshDetails();
                }
            } else {
                updatePie();
            }
        }

        function refreshDetails() {
            stopTimer();

            vm.timer.seconds = 0;
            vm.timer.mins = 0;
            updatePie();

            updateDetails();
        }

        function updatePie() {
            if (vm.timer.seconds > 0 && vm.timer.total > 0) {
                var degs = 360 - 360 * (vm.timer.seconds / vm.timer.total);
                vm.pie.transform1 = 'rotate(' + (degs > 180 ? 180 : degs) + 'deg) translate(0, -25%)';
                vm.pie.transform2 = 'rotate(' + (degs > 180 ? degs - 180 : 0) + 'deg) translate(0, -25%)';
            } else {
                vm.pie.degs1 = 0;
                vm.pie.degs2 = 0;
            }
        }

        function stopTimer() {
            if (angular.isDefined(vm.timer.interval)) {
                $interval.cancel(vm.timer.interval);
                vm.timer.interval = undefined;
            }
        }

        function stopStatusTimeout() {
            if (vm.status.timeout)
                $timeout.cancel(vm.status.timeout);

            vm.status.timeout = undefined;
        }

        function startStatusTimeout() {
            stopStatusTimeout();
            vm.status.timeout = $timeout(updateStatus, 15000);
        }

        function getFile(file) {
            apiSvc.getFile(vm.model.id, file.id);
        }

        function gotoCallbackUrl(status) {
            if (status === 'Paid' && vm.callback.url) {
                $window.location.href = vm.callback.url;
                return true;
            } else if (status === 'Removed') {
                $window.location.href = $window.location.href;
            }
            return false;
        }

        function getParameterByName(name, url) {
            if (!url) url = window.location.href;
            name = name.replace(/[\[\]]/g, "\\$&");
            var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        }
    }
})();
