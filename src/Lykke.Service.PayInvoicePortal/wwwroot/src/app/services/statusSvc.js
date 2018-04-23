(function () {
    'use strict';

    angular
        .module('app')
        .service('statusSvc', statusSvc);

    statusSvc.$inject = [];

    function statusSvc() {
        var service = {
            getStatusCss: getStatusCss
        };

        return service;

        function getStatusCss(status) {
            switch (status) {
                case 'Draft':
                    return 'label--gray';
                case 'Unpaid':
                    return 'label--yellow';
                case 'Removed':
                    return '';
                case 'InProgress':
                case 'RefundInProgress':
                    return 'label--blue';
                case 'Paid':
                    return 'label--green';
                case 'Underpaid':
                case 'Overpaid':
                case 'LatePaid':
                    return 'label--violet';
                case 'Refunded':
                    return 'label--dark';
                case 'NotConfirmed':
                case 'InternalError':
                    return 'label--red';
            }
            return '';
        }
    }
})();