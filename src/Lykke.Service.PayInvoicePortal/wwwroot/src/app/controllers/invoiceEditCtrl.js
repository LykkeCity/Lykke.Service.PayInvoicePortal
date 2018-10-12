﻿(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoiceEditCtrl', invoiceEditCtrl);

    invoiceEditCtrl.$inject = ['$scope', '$window', '$log', '$rootScope', '$interval', 'apiSvc', 'statusSvc', 'fileSvc', 'confirmModalSvc'];

    function invoiceEditCtrl($scope, $window, $log, $rootScope, $interval, apiSvc, statusSvc, fileSvc, confirmModalSvc) {
        var vm = this;

        vm.form = {
            open: false,
            submited: false,
            errors: [],
            blocked: false
        };

        vm.model = {
            id: '',
            status: '',
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
            generate: generate,
            saveDraft: saveDraft,
            getFileExtension: fileSvc.getExtension,
            getFileSize: fileSvc.getSize,
            getFile: getFile,
            upload: upload,
            deleteFile: deleteFile
        };

        activate();

        function activate() {
            $scope.$on('$destroy',
                function () {
                    destroy();
                });

            pubsubEvents.on('invoiceDraftEdit', invoiceDraftEditEventHandler);

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
            pubsubEvents.off('invoiceDraftEdit', invoiceDraftEditEventHandler);
        }

        function invoiceDraftEditEventHandler(data) {
            $scope.$apply(function() {
                open(data);
            })
        }

        function open(data) {
            $rootScope.blur = true;
            vm.form.open = true;

            reset();

            vm.model.id = data.id;
            vm.model.status = data.status;
            vm.model.number = data.number;
            vm.model.client = data.client;
            vm.model.email = data.email;
            vm.model.settlementAsset = data.settlementAsset;
            vm.model.amount = data.amount;
            vm.model.dueDate = $window.moment(data.dueDate);
            vm.model.note = data.note;
            vm.model.files = [];

            angular.forEach(data.files, function (file, key) {
                vm.model.files.push(file);
            });
        }

        function updateFiles() {
            apiSvc.getInvoice(vm.model.id)
                .then(
                    function (data) {
                        vm.model.files = data.files;
                    },
                    function (error) {
                        $log.error(error);
                    });
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
                    id: vm.model.id,
                    number: vm.model.number,
                    client: vm.model.client,
                    email: vm.model.email,
                    amount: vm.model.amount,
                    settlementAsset: vm.model.settlementAsset,
                    dueDate: vm.model.dueDate.endOf('day').toDate(),
                    note: vm.model.note
                };

            apiSvc.updateInvoice(model)
                .then(
                    function (data) {
                        close();
                        onChanged();
                        if (isDraft === false) {
                            $rootScope.$broadcast('invoiceGenerated', data);
                        }
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

        function upload(files) {
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

            apiSvc.uploadFile(vm.model.id, files)
                .then(
                    function () {
                        updateFiles();
                        onChanged();
                    },
                    function (error) {
                        $log.error(error);
                    });
        }

        function getFile(file) {
            apiSvc.getFile(vm.model.id, file.id);
        }

        function deleteFile(file) {
            if (!file)
                return;

            confirmModalSvc.open({
                content: 'Are you sure you want to remove this attachment "' + file.name + '"?',
                yesAction: function () {
                    apiSvc.deleteFile(vm.model.id, file.id)
                        .then(
                            function () {
                                updateFiles();
                                onChanged();
                            },
                            function (error) {
                                $log.error(error);
                            });
                }
            });
        }

        function onChanged() {
            $rootScope.$broadcast('invoiceDraftUpdated', {});

            pubsubEvents.emit('invoiceUpdated', {});
        }
    }
})();
