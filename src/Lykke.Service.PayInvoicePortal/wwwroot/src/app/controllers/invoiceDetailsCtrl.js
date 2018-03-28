(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoiceDetailsCtrl', invoiceDetailsCtrl);

    invoiceDetailsCtrl.$inject = ['$scope', '$window', '$log', '$rootScope', 'apiSvc', 'statusSvc', 'fileSvc'];

    function invoiceDetailsCtrl($scope, $window, $log, $rootScope, apiSvc, statusSvc, fileSvc) {
        var vm = this;

        vm.form = {
            allowDelete: false,
            allowEdit: false,
            allowPay: false
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

        vm.handlers = {
            canEdit: canEdit,
            edit: edit,
            canRemove: canRemove,
            remove: remove,
            init: init,
            getFileExtension: fileSvc.getExtension,
            getFileSize: fileSvc.getSize,
            getFile: getFile,
            upload: upload,
            deleteFile: deleteFile
        };

        activate();

        function activate() {
            $scope.$on('invoiceDraftUpdated', function (evt, data) {
                update();
            });
        }

        function init(data) {
            apply(data);
        }
        
        function canEdit() {
            return vm.model.status === 'Draft';
        }

        function edit() {
            if (!canEdit())
                return;

            $rootScope.$broadcast('invoiceDraftEdit',
                {
                    id: vm.model.id,
                    status: vm.model.status,
                    number: vm.model.number,
                    client: vm.model.client,
                    email: vm.model.email,
                    settlementAsset: vm.model.settlementAsset,
                    amount: vm.model.amount,
                    dueDate: vm.model.dueDate,
                    note: vm.model.note,
                    files: vm.model.files
                });
        }

        function canRemove() {
            return vm.model.status === 'Draft' || vm.model.status === 'Unpaid';
        }

        function remove() {
            if (!canRemove())
                return;

            $.confirm({
                title: 'Are you sure?',
                content: 'Do you really want to delete this invoice?',
                icon: 'fa fa-question-circle',
                animation: 'scale',
                closeAnimation: 'scale',
                opacity: 0.5,
                buttons: {
                    'confirm': {
                        text: 'Yes',
                        btnClass: 'btn-blue',
                        action: function () {
                            apiSvc.deleteInvoice(vm.model.id)
                                .then(
                                    function () {
                                        $window.history.back();
                                    },
                                    function (error) {
                                        $log.error(error);
                                    });
                        }
                    },
                    cancel: function () {
                    }
                }
            });
        }

        function apply(data) {
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
            vm.model.url = $window.location.origin + '/invoice/' + vm.model.id;
            vm.model.files = data.files;

            vm.form.allowPay = data.status === 'Unpaid';
            vm.form.allowEdit = data.status === 'Draft';
            vm.form.allowDelete = data.status === 'Draft' || data.status === 'Unpaid';
        }

        function update() {
            apiSvc.getInvoice(vm.model.id)
                .then(
                    function (data) {
                        apply(data);
                    },
                    function (error) {
                        $log.error(error);
                    });
        }

        function getFile(file) {
            apiSvc.getFile(vm.model.id, file.id);
        }

        function upload(files) {
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

            apiSvc.uploadFile(vm.model.id, files)
                .then(
                    function(data) {
                        update(data);
                    },
                    function(error) {
                          $log.error(error);
                    });
        }

        function deleteFile(file) {
            if (!file)
                return;

            $.confirm({
                title: 'Are you sure?',
                content: 'Do you really want to delete "' + file.name + '"?',
                icon: 'fa fa-question-circle',
                animation: 'scale',
                closeAnimation: 'scale',
                opacity: 0.5,
                buttons: {
                    'confirm': {
                        text: 'Yes',
                        btnClass: 'btn-blue',
                        action: function () {
                            apiSvc.deleteFile(vm.model.id, file.id)
                                .then(
                                    function () {
                                        update();
                                    },
                                    function (error) {
                                        $log.error(error);
                                    });
                        }
                    },
                    cancel: function () {
                    }
                }
            });
        }
    }
})();