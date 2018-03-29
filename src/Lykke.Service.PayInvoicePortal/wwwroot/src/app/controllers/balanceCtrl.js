(function() {
    'use strict';

    angular
        .module('app')
        .controller('balanceCtrl', balanceCtrl);

    balanceCtrl.$inject = ['$scope', '$log', '$interval', 'apiSvc'];

    function balanceCtrl($scope, $log, $interval, apiSvc) {
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
            $scope.$on('$destroy',
                function () {
                    destroy();
                });

            vm.intervals.balance = $interval(update, 3 * 60 * 1000);
            update();
        }

        function destroy() {
            if (angular.isDefined(vm.intervals.balance)) {
                $interval.cancel(vm.intervals.balance);
                vm.intervals.balance = undefined;
            }
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