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
            errors: [],
            blocked: false
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
            getFileSize: fileSvc.getSize,
            addFiles: addFiles,
            deleteFile: deleteFile
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
            if (vm.form.blocked)
                return;

            if (!validate())
                return;
            
            vm.form.blocked = true;

            var model =
            {
                isDraft: false,
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
                        $rootScope.$broadcast('invoiceGenerated', data);
                        vm.form.blocked = false;
                    },
                    function (error) {
                        $log.error(error);
                        vm.form.blocked = false;
                    });
        }

        function draft() {
            if (vm.form.blocked)
                return;

            if (!validate())
                return;

            vm.form.blocked = true;

            var model =
            {
                isDraft: true,
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
                        $rootScope.$broadcast('invoiceDraftCreated', data);
                        vm.form.blocked = false;
                    },
                    function (error) {
                        $log.error(error);
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
                $.confirm({
                    title: 'Invalid file',
                    content: fileSvc.getError(),
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    closeAnimation: 'scale',
                    opacity: 0.5,
                    buttons: {
                        'ok': {
                            text: 'OK',
                            btnClass: 'btn-blue'
                        }
                    }
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