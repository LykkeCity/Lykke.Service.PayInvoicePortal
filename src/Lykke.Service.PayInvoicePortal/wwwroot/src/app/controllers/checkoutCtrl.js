(function () {
    'use strict';

    angular
        .module('app')
        .controller('checkoutCtrl', checkoutCtrl);

    checkoutCtrl.$inject = ['$window', '$scope', '$log', '$interval', '$timeout', 'apiSvc', 'fileSvc'];

    function checkoutCtrl($window, $scope, $log, $interval, $timeout, apiSvc, fileSvc) {
        var vm = this;

        vm.pie = {
            transform1: '',
            transform2: ''
        };

        vm.timer = {
            interval: null,
            total: 0,
            seconds: 0,
            mins: 0
        };

        vm.status = {
            timeout: null
        }

        vm.model = {
            id: '',
            number: '',
            merchant: '',
            paymentAmount: 0,
            settlementAmount: 0,
            paymentAsset: '',
            settlementAsset: '',
            paymentAssetAccuracy: 0,
            settlementAssetAccuracy: 0,
            exchangeRate: 0,
            spreadPercent: 0,
            feePercent: 0,
            dueDate: $window.moment(),
            note: '',
            qrCode: '',
            walletAddress: '',
            files: [],
            waiting: false
        };

        vm.handlers = {
            init: init,
            getFileExtension: fileSvc.getExtension,
            getFileSize: fileSvc.getSize,
            getFile: getFile
        };

        activate();

        function activate() {
            $scope.$on('$destroy',
                function() {
                    stopTimer();
                    stopStatusTimeout();
                });
        }

        function init(data) {
            apply(data);
            startStatusTimeout();
        }

        function apply(data) {
            vm.model.id = data.id;
            vm.model.number = data.number;
            vm.model.status = data.status;
            vm.model.merchant = data.merchant;
            vm.model.paymentAmount = data.paymentAmount;
            vm.model.settlementAmount = data.settlementAmount;
            vm.model.paymentAsset = data.paymentAsset;
            vm.model.settlementAsset = data.settlementAsset;
            vm.model.paymentAssetAccuracy = data.paymentAssetAccuracy;
            vm.model.settlementAssetAccuracy = data.settlementAssetAccuracy;
            vm.model.exchangeRate = data.exchangeRate;
            vm.model.spreadPercent = data.spreadPercent;
            vm.model.feePercent = data.feePercent;
            vm.model.dueDate = $window.moment(data.dueDate);
            vm.model.note = data.note;
            vm.model.walletAddress = data.walletAddress;
            vm.model.files = data.files;

            if (data.status === 'Unpaid' && !data.expired) {
                vm.model.qrCode = 'https://chart.googleapis.com/chart?chs=220x220&chld=L|2&cht=qr&chl=bitcoin:' +
                    data.walletAddress +
                    '?amount=' +
                    data.paymentAmount +
                    '&label=invoice #' +
                    data.number +
                    '&message=' +
                    data.paymentRequestId;

                vm.timer.total = data.totalSeconds;
                vm.timer.seconds = data.remainingSeconds;
                vm.model.waiting = true;
                updatePie();

                vm.timer.interval = $interval(tick, 1000);
            } else {
                vm.model.qrCode = '';
                vm.timer.total = 0;
                vm.timer.seconds = 0;
                vm.model.waiting = false;
            }
        }

        function updateDetails() {
            apiSvc.getPaymentDetails(vm.model.id)
                .then(
                    function (data) {
                        apply(data);
                    },
                    function (error) {
                        $log.error(error);
                    });
        }

        function updateStatus() {
            apiSvc.getInvoiceStatus(vm.model.id)
                .then(
                    function (data) {
                        if (data.status !== vm.model.status) {
                            stopTimer();
                            updateDetails();
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
            vm.timer.mins = vm.timer.seconds / 60 | 0;

            if (vm.timer.seconds <= 0) {
                stopTimer();
                updateDetails();

                vm.timer.seconds = 0;
                vm.timer.mins = 0;
            }

            updatePie();
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
    }
})();