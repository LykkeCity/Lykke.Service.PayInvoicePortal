(function() {
    'use strict';

    angular
        .module('app')
        .controller('invoiceCtrl', invoiceCtrl);

    invoiceCtrl.$inject = ['$scope', '$window', '$log', '$rootScope', '$interval', 'apiSvc', 'statusSvc', 'fileSvc', 'confirmModalSvc'];

    function invoiceCtrl($scope, $window, $log, $rootScope, $interval, apiSvc, statusSvc, fileSvc, confirmModalSvc) {
        var vm = this;

        vm.events = {
            createInvoice: undefined
        };

        vm.form = {
            open: false,
            submited: false,
            errors: [],
            blocked: false
        };

        vm.model = {
            number: '',
            client: '',
            email: '',
            settlementAsset: null,
            amount: 0,
            dueDate: $window.moment().add(2, 'days'),
            note: '',
            files: [],
            assets: []
        };

        vm.handlers = {
            close: close,
            openNewForm: openNewForm,
            generate: generate,
            saveDraft: saveDraft,
            getFileExtension: fileSvc.getExtension,
            getFileSize: fileSvc.getSize,
            addFiles: addFiles,
            deleteFile: deleteFile
        };

        activate();
        
        function activate() {
            $scope.$on('$destroy',
                function () {
                    destroy();
                });

            vm.events.createInvoice = $scope.$on('createInvoice', function (evt, data) {
                vm.handlers.openNewForm(data);
            });

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

        function destroy() {
            if (vm.events.createInvoice)
                vm.events.createInvoice();
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
            vm.model.dueDate = $window.moment().add(2, 'days');
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

        function generate() {
            // isDraft: false
            save(false);
        }

        function saveDraft() {
            // isDraft: true
            save(true);
        }

        function save(isDraft) {
            if (vm.form.blocked)
                return;

            if (!validate())
                return;

            vm.form.blocked = true;

            var model =
            {
                isDraft: isDraft,
                number: vm.model.number,
                client: vm.model.client,
                email: vm.model.email,
                amount: vm.model.amount,
                settlementAsset: vm.model.settlementAsset,
                dueDate: vm.model.dueDate.endOf('day').toDate(),
                note: vm.model.note
            };

            apiSvc.saveInvoice(model, vm.model.files)
                .then(
                    function (data) {
                        close();
                        isDraft ? $rootScope.$broadcast("invoiceDraftCreated", data) : $rootScope.$broadcast("invoiceGenerated", data);
                    },
                    function (error) {
                        $log.error(error);

                        confirmModalSvc.open({
                            title: confirmModalSvc.constants.errorTitle,
                            content: confirmModalSvc.constants.errorCommonMessage
                        });
                    })
                .finally(
                    function () {
                        vm.form.blocked = false;
                    });
        }

        function addFiles(files) {
            if (!files || files.length === 0)
                return;

            var valid = true;
            angular.forEach(files, function (file, key) {
                valid = valid && fileSvc.validate(file);
            });

            if (!valid) {
                confirmModalSvc.open({
                    title: 'Invalid file',
                    content: fileSvc.getError()
                });

                return;
            }

            angular.forEach(files, function (file, key) {
                vm.model.files.push(file);
            });
        }

        function deleteFile(file) {
            var index = vm.model.files.indexOf(file);

            if (index >= 0) {
                vm.model.files.splice(index, 1);
            }
        }
    }
})();
