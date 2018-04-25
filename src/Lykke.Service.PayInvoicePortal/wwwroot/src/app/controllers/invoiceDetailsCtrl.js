(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoiceDetailsCtrl', invoiceDetailsCtrl);

    invoiceDetailsCtrl.$inject = ['$scope', '$window', '$log', '$rootScope', 'apiSvc', 'statusSvc', 'fileSvc', 'confirmModalSvc'];

    function invoiceDetailsCtrl($scope, $window, $log, $rootScope, apiSvc, statusSvc, fileSvc, confirmModalSvc) {
        var vm = this;

        vm.events = {
            invoiceDraftUpdated: undefined
        };

        vm.form = {
            allowDelete: false,
            allowEdit: false,
            allowPay: false,
            showBcnLink: false
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
            walletAddress: '',
            note: '',
            url: '',
            files: [],
            history: []
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
            getStatusCss: statusSvc.getStatusCss,
            upload: upload,
            deleteFile: deleteFile,
            getInitials: getInitials
        };

        activate();

        function activate() {
            $scope.$on('$destroy',
                function () {
                    destroy();
                });

            vm.events.invoiceDraftUpdated = $scope.$on('invoiceDraftUpdated', function (evt, data) {
                update();
            });
        }

        function destroy() {
            if (vm.events.invoiceDraftUpdated)
                vm.events.invoiceDraftUpdated();
        }

        function getInitials(author) {
            var value = '';

            if (!author || author.length === 0)
                return value;

            var parts = author.split(' ');
            
            if (parts.length > 0)
                value = value + parts[0][0].toUpperCase();

            if (parts.length > 1)
                value = value + parts[1][0].toUpperCase();

            return value;
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

            confirmModalSvc.open({
                content: 'Are you sure you want to remove this invoice "#' + vm.model.number + '"?',
                yesAction: function () {
                    apiSvc.deleteInvoice(vm.model.id)
                        .then(
                            function () {
                                $window.history.back();
                            },
                            function (error) {
                                $log.error(error);
                            });
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
            vm.model.walletAddress = data.walletAddress;
            vm.model.note = data.note;
            vm.model.url = $window.location.origin + '/invoice/' + vm.model.id;
            vm.model.files = data.files;

            angular.forEach(data.history, function (item, key) {
                item.dueDate = $window.moment(item.dueDate);
                item.date = $window.moment(item.date);
                if (item.paidDate)
                    item.paidDate = $window.moment(item.paidDate);
            });
            

            vm.model.history = data.history;

            vm.form.allowPay = data.status === 'Unpaid';
            vm.form.allowEdit = data.status === 'Draft';
            vm.form.allowDelete = data.status === 'Draft' || data.status === 'Unpaid';
            vm.form.showBcnLink = vm.model.walletAddress 
                && ['InProgress', 'Paid', 'Underpaid', 'Overpaid', 'LatePaid', 'RefundInProgress', 'Refunded', 'NotConfirmed', 'InternalError'].indexOf(vm.model.status) > -1;
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
                confirmModalSvc.open({
                    title: 'Invalid file',
                    content: fileSvc.getError()
                });

                return;
            }

            vm.form.blocked = true;

            apiSvc.uploadFile(vm.model.id, files)
                .then(
                    function(data) {
                        update(data);
                        vm.form.blocked = false;
                    },
                    function(error) {
                        $log.error(error);
                        vm.form.blocked = false;
                    });
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
                                update();
                            },
                            function (error) {
                                $log.error(error);
                            });
                }
            });
        }
    }
})();