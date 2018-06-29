(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoicesNavCtrl', invoicesNavCtrl);

        invoicesNavCtrl.$inject = ['$window'];

    function invoicesNavCtrl($window) {
        var vm = this;

        vm.model = {
            isHome: $window.location.pathname === '/',
            isIncoming: $window.location.pathname === '/incoming'
        };
    }
})();
