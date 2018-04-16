(function() {
    'use strict';

    angular
        .module('app')
        .controller('invoiceInfoCtrl', invoiceInfoCtrl);

    invoiceInfoCtrl.$inject = ['$scope', '$window', '$log', '$rootScope', 'apiSvc', 'statusSvc', 'fileSvc'];

    function invoiceInfoCtrl($scope, $window, $log, $rootScope, apiSvc, statusSvc, fileSvc) {
        var vm = this;

        vm.events = {
            invoiceGenerated: undefined
        };

        vm.form = {
            open: false,
            submited: false,
            errors: []
        };
        
        vm.model = {
            id: '',
            status: '',
            number: '',
            client: '',
            email: '',
            settlementAsset: null,
            settlementAssetAccuracy: 0,
            amount: 0,
            dueDate: null,
            note: '',
            url: '',
            files: []
        };

        vm.share = {
            email: '',
            sent: false,
            blocked: false,
            handler: undefined
        };

        vm.handlers = {
            close: close,
            openForm: openForm,
            share: share,
            tooltipHandler: tooltipHandler
        };

        activate();

        function activate() {
            $scope.$on('$destroy',
                function() {
                    destroy();
                });

            vm.events.invoiceGenerated = $scope.$on('invoiceGenerated', function (evt, data) {
                vm.handlers.openForm(data);
            });
        }

        function destroy() {
            if (vm.events.invoiceGenerated)
                vm.events.invoiceGenerated();
        }

        function close() {
            if (vm.form.open) {
                $rootScope.blur = false;
                vm.form.open = false;
            }
        }

        function openForm(data) {
            if (!data)
                return;

            vm.model.id = data.id;
            vm.model.status = data.status;
            vm.model.number = data.number;
            vm.model.client = data.clientName;
            vm.model.email = data.clientEmail;
            vm.model.settlementAsset = data.settlementAsset;
            vm.model.settlementAssetAccuracy = data.settlementAssetAccuracy;
            vm.model.amount = data.amount;
            vm.model.dueDate = $window.moment(data.dueDate);
            vm.model.note = data.note;
            vm.model.url = getUrl();
            vm.model.files = data.files;

            vm.share.email = data.clientEmail;
            vm.share.sent = false;

            $rootScope.blur = true;
            vm.form.open = true;
        }

        function validate() {
            vm.form.errors = [];

            if (!vm.share.email) {
                vm.form.errors['email'] = true;
            } else {
                var emails = vm.share.email.split(',');

                var exp = /[A-z0-9._%+-]+@[A-z0-9.-]+\.[A-z]{2,3}$/i;

                var valid = true;
                angular.forEach(emails, function (email, key) {
                    valid = valid && exp.exec(email);
                });

                vm.form.errors['email'] = !valid;
            }

            for (var key in vm.form.errors) {
                if (vm.form.errors.hasOwnProperty(key)) {
                    if (vm.form.errors[key] === true)
                        return false;
                }
            }

            return true;
        }

        function share() {
            if (vm.share.blocked)
                return;

            if (!validate())
                return;

            vm.share.blocked = true;

            var model = {
                invoiceId: vm.model.id,
                checkoutUrl: getUrl(),
                emails: vm.share.email.split(',')
            };

            apiSvc.sendEmail(model)
                .then(
                    function () {
                        vm.share.sent = true;
                        vm.share.email = '';
                        vm.share.blocked = false;

                        if (vm.share.handler)
                            vm.share.handler();
                    },
                    function (error) {
                        $log.error(error);
                        vm.share.blocked = false;
                    });
        }

        function tooltipHandler(handler) {
            vm.share.handler = handler;
        }

        function getUrl() {
            return $window.location.origin + '/invoice/' + vm.model.id;
        }
    }
})();