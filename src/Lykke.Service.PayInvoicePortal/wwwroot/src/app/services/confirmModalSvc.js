(function () {
    'use strict';

    angular
        .module('app')
        .service('confirmModalSvc', confirmModalSvc);

    confirmModalSvc.$inject = ['$rootScope'];

    function confirmModalSvc($rootScope) {
        var service = {
            open: open,
            excludeClickClassList: getExcludeClickClassList()
        };

        return service;

        function open(data) {
            $rootScope.$broadcast('openConfirmModal', data);
        }

        function getExcludeClickClassList() {
            return [
                'menu_overlay--open',
                'modal_dialog--open',
                'modal_dialog__btn'
            ];
        }
    }
})();