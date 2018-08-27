(function () {
    'use strict';

    angular
        .module('app')
        .controller('signRequestCtrl', signRequestCtrl);

    signRequestCtrl.$inject = ['$log', '$rootScope', 'apiSvc', 'fileSvc', 'confirmModalSvc'];

    function signRequestCtrl($log, $rootScope, apiSvc, fileSvc, confirmModalSvc) {
        var vm = this;

        vm.view = {
            isLoading: false,
            isSidebarVisible: false
        };

        vm.form = {
            errors: []
        };

        vm.model = {
            lykkeMerchantId: '',
            apiKey: '',
            rsaPrivateKey: '',
            body: '',
            result: ''
        };

        vm.handlers = {
            init: init,
            submit: submit,
            open: open,
            close: close
        };

        function init(data) {
            vm.model.lykkeMerchantId = data.merchantId;
            vm.model.apiKey = data.apiKey;
        }

        function validate() {
            vm.form.errors = [];

            vm.form.errors['lykkeMerchantId'] = !vm.model.lykkeMerchantId;
            vm.form.errors['apiKey'] = !vm.model.apiKey;
            vm.form.errors['rsaPrivateKey'] = !vm.model.rsaPrivateKey;
            vm.form.errors['body'] = !vm.model.body;

            for (var key in vm.form.errors) {
                if (vm.form.errors.hasOwnProperty(key)) {
                    if (vm.form.errors[key] === true)
                        return false;
                }
            }

            return true;
        }

        function submit() {
            if (!validate())
                return;

            vm.view.isLoading = true;
            vm.model.result = '';

            var model = {
                lykkeMerchantId: vm.model.lykkeMerchantId,
                apiKey: vm.model.apiKey,
                rsaPrivateKey: vm.model.rsaPrivateKey,
                body: vm.model.body
            };

            apiSvc.signRequest(model)
                .then(
                    function (data) {
                        vm.model.result = data.signedBody;
                    },
                    function (error) {
                        $log.error(error);

                        confirmModalSvc.open({
                            title: confirmModalSvc.constants.errorTitle,
                            content: 'Unable to sign request with provided data. Please review input data or contact support.'
                        });
                    })
                .finally(
                    function () {
                        vm.view.isLoading = false;
                    });
        }

        function open() {
            $rootScope.blur = true;
            vm.view.isSidebarVisible = true;
        }

        function close() {
            $rootScope.blur = false;
            vm.view.isSidebarVisible = false;
        }
    }
})();
