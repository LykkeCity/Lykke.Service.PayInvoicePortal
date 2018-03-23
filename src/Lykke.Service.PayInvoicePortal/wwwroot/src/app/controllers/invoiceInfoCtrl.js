(function() {
    'use strict';

    angular
        .module('app')
        .controller('invoiceInfoCtrl', invoiceInfoCtrl);

    invoiceInfoCtrl.$inject = ['$scope', '$window', '$log', '$rootScope', 'apiSvc', 'statusSvc', 'fileSvc'];

    function invoiceInfoCtrl($scope, $window, $log, $rootScope, apiSvc, statusSvc, fileSvc) {
        var vm = this;

        vm.form = {
            open: false
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
            close: close,
            openForm: openForm,
            share: share
        };

        $scope.$on('invoiceGenerated', function (evt, data) {
            vm.handlers.openForm(data);
        });

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
            vm.model.url = $window.location.origin + '/invoice/' + vm.model.id;
            vm.model.files = data.files;

            $rootScope.blur = true;
            vm.form.open = true;
        }

        function share(url) {
            
        }
    }
})();