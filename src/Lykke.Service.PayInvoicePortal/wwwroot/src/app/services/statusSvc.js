(function () {
    'use strict';

    angular
        .module('app')
        .service('statusSvc', statusSvc);

    statusSvc.$inject = [];

    function statusSvc() {
        var service = {
            getStatusRowCss: getStatusRowCss
        };

        return service;

        function getStatusRowCss(status) {
            switch (status) {
                case 'Draft':
                    return 'label--gray';
                case 'Unpaid':
                    return 'label--yellow';
                case 'Removed':
                    return '';
                case 'InProgress':
                case 'RefundInProgress':
                case 'SettlementInProgress':
                    return '';
                case 'Paid':
                    return 'label--green';
                case 'UnderPaid':
                case 'OverPaid':
                case 'LatePaid':
                    return 'label--violet';
                case 'Refunded':
                case 'Settled':
                    return 'label--blue';
                case 'NotConfirmed':
                case 'InvalidAddress':
                case 'InternalError':
                    return 'label--red';
            }
            return '';
        }
    }
})();