(function() {
    'use strict';

    angular
        .module('app')
        .controller('invoicesSummaryStatisticCtrl', invoicesSummaryStatisticCtrl);

        invoicesSummaryStatisticCtrl.$inject = ['$scope', '$rootScope'];

    function invoicesSummaryStatisticCtrl($scope, $rootScope) {
        var vm = this;

        vm.events = {
            openSummaryStatistic: undefined
        };

        vm.statistic = {
            open: false,
            summary: []
        };

        vm.handlers = {
            close: close
        };

        activate();

        function activate() {
            vm.events.openSummaryStatistic = $scope.$on('openSummaryStatistic', function (evt, data) {
                vm.statistic.summary = data;
                vm.statistic.open = true;
                $rootScope.blur = true;
            });

            $scope.$on('$destroy', function () {
                if (vm.events.openSummaryStatistic)
                    vm.events.openSummaryStatistic();
            });
        }

        function close() {
            if (vm.statistic.open) {
                vm.statistic.open = false;
                $rootScope.blur = false;
            }
        }
    }
})();
