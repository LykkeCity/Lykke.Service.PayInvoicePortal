(function () {
    'use strict';

    angular
        .module('app')
        .service('statusSvc', statusSvc);

    statusSvc.$inject = [];

    function statusSvc() {
        var service = {
            getStatusCss: getStatusCss,
            getAlertStatusCss: getAlertStatusCss
        };

        return service;

        function getAlertStatusCss(status) {
            return getStatusCss(status, 'alert');
        }
    
        function getStatusCss(status, prefix) {
            if (!prefix) {
                prefix = 'label';
            }

            switch (status) {
                case 'Draft':
                    return prefix + '--gray';
                case 'Unpaid':
                    return prefix + '--yellow';
                case 'Removed':
                    return '';
                case 'InProgress':
                case 'RefundInProgress':
                    return prefix + '--blue';
                case 'Paid':
                    return prefix + '--green';
                case 'Underpaid':
                case 'Overpaid':
                case 'LatePaid':
                    return prefix + '--violet';
                case 'Refunded':
                    return prefix + '--dark';
                case 'NotConfirmed':
                case 'InternalError':
                    return prefix + '--red';
            }
            return '';
        }
    }
})();