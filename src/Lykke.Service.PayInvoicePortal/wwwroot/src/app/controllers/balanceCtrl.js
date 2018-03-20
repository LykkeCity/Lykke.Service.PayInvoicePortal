(function() {
    'use strict';

    angular
        .module('app')
        .controller('balanceCtrl', balanceCtrl);

    balanceCtrl.$inject = ['$log', '$interval', 'apiSvc'];

    function balanceCtrl($log, $interval, apiSvc) {
        var vm = this;

        vm.intervals = {
            balance: null
        };

        vm.model = {
            value: 0,
            currency: ''
        };

        activate();

        function activate() {
            vm.intervals.balance = $interval(update, 3 * 60 * 1000);
            update();
        }

        function update() {
            apiSvc.getBalance()
                .then(
                    function(data) {
                        vm.model.value = data.value;
                        vm.model.currency = data.currency;
                    },
                    function(error) {
                        $log.error(error);
                    });
        }
    }
})();