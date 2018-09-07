(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoicesNavCtrl', invoicesNavCtrl);

        invoicesNavCtrl.$inject = ['$window'];

    function invoicesNavCtrl($window) {
        var vm = this;

        vm.model = {
            isHome: $window.location.pathname === '/payments',
            isIncoming: $window.location.pathname === '/incoming'
        };

        vm.handlers = {
            isPath: isPath
        };

        function isPath(path) {
            return $window.location.pathname === path;
        }
    }
})();
