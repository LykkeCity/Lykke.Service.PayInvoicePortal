(function () {
    'use strict';

    angular
        .module('app')
        .controller('subscriptionCtrl', subscriptionCtrl);

    subscriptionCtrl.$inject = ['$log', 'apiSvc'];

    function subscriptionCtrl($log, apiSvc) {
        var vm = this;

        vm.model = {
            email: '',
            sent: false,
            blocked: false,
            error: ''
        };

        vm.handlers = {
            subscribe: subscribe
        };

        activate();

        function activate() {
        }

        function subscribe() {
            vm.model.sent = false;
            vm.model.error = '';

            if (!vm.model.email || vm.model.blocked)
                return;

            vm.model.blocked = true;

            var model = {
                email: vm.model.email
            };

            apiSvc.subscribe(model)
                .then(
                    function(data) {
                        if (data.error === true) {
                            vm.model.error = data.message;
                        } else {
                            vm.model.email = '';
                            vm.model.sent = true;
                        }
                        vm.model.blocked = false;
                    },
                    function(error) {
                        $log.error(error);
                        vm.model.blocked = false;
                    });
        }
    }
})();