(function() {
    'use strict';

    angular
        .module('app')
        .controller('invoiceCtrl', invoiceCtrl);

    invoiceCtrl.$inject = ['$scope', '$window', '$log', '$rootScope', '$interval', 'apiSvc', 'statusSvc', 'fileSvc'];

    function invoiceCtrl($scope, $window, $log, $rootScope, $interval, apiSvc, statusSvc, fileSvc) {
        var vm = this;

        vm.form = {
            open: false,
            submited: false,
            errors: []
        };

        vm.model = {
            number: '',
            client: '',
            email: '',
            settlementAsset: null,
            amount: 0,
            dueDate: $window.moment(),
            note: '',
            files: [],
            assets: []
        };

        vm.handlers = {
            close: close,
            openNewForm: openNewForm,
            save: save,
            draft: draft,
            getFileExtension: fileSvc.getExtension,
            getFileSize: fileSvc.getSize
        };

        $scope.$on('createInvoice', function (evt, data) {
            vm.handlers.openNewForm(data);
        });

        activate();
        
        function activate() {
            apiSvc.getAssets()
                .then(
                    function (data) {
                        vm.model.assets = data || [];

                        if (vm.model.assets.length > 0)
                            vm.model.settlementAsset = vm.model.assets[0].id;
                    },
                    function (error) {
                        $log.error(error);
                    });
        }

        function openNewForm(data) {
            $rootScope.blur = true;
            vm.form.open = true;

            reset();
        }

        function reset() {
            vm.model.number = '';
            vm.model.client = '';
            vm.model.email = '';

            if (vm.model.assets.length > 0)
                vm.model.settlementAsset = vm.model.assets[0].id;
            else
                vm.model.settlementAsset = null;

            vm.model.amount = 0;
            vm.model.dueDate = $window.moment();
            vm.model.note = '';
            vm.model.files = [];

            vm.form.errors = [];
        }

        function close() {
            if (vm.form.open) {
                $rootScope.blur = false;
                vm.form.open = false;
                reset();
            }
        }

        function validate() {
            vm.form.errors = [];

            vm.form.errors['number'] = !vm.model.number;
            vm.form.errors['client'] = !vm.model.client;
            vm.form.errors['email'] = !vm.model.email;
            vm.form.errors['settlementAsset'] = !vm.model.settlementAsset;
            vm.form.errors['amount'] = !vm.model.amount || vm.amount <= 0.01;
            vm.form.errors['dueDate'] = !vm.model.dueDate || vm.model.dueDate.diff(new Date(), "days") < 0;

            for (var key in vm.form.errors) {
                if (vm.form.errors.hasOwnProperty(key)) {
                    if (vm.form.errors[key] === true)
                        return false;
                }
            }

            return true;
        }

        function save() {
            if (!validate())
                return;

            var model =
            {
                isDraft: false,
                number: vm.model.number,
                client: vm.model.client,
                email: vm.model.email,
                amount: vm.model.amount,
                settlementAsset: vm.model.settlementAsset,
                dueDate: vm.model.dueDate.toDate(),
                note: vm.model.note
            };

            apiSvc.saveInvoice(model, vm.model.files)
                .then(
                    function (data) {
                        close();
                        $rootScope.$broadcast('invoiceGenerated', data);
                    },
                    function (error) {
                        $log.error(error);
                    });
        }

        function draft() {
            if (!validate())
                return;

            var model =
            {
                isDraft: true,
                number: vm.model.number,
                client: vm.model.client,
                email: vm.model.email,
                amount: vm.model.amount,
                settlementAsset: vm.model.settlementAsset,
                dueDate: vm.model.dueDate.toDate(),
                note: vm.model.note
            };

            apiSvc.saveInvoice(model, vm.model.files)
                .then(
                    function (data) {
                        close();
                        $rootScope.$broadcast('invoiceDraftCreated', data);
                    },
                    function (error) {
                        $log.error(error);
                    });
        }
    }
})();