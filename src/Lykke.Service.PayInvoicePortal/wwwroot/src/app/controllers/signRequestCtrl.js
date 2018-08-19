(function () {
    'use strict';

    angular
        .module('app')
        .controller('signRequestCtrl', signRequestCtrl);

    signRequestCtrl.$inject = ['$log', 'apiSvc', 'fileSvc', 'confirmModalSvc'];

    function signRequestCtrl($log, apiSvc, fileSvc, confirmModalSvc) {
        var vm = this;

        vm.view = {
            isLoading: false
        };

        vm.form = {
            errors: []
        };

        vm.model = {
            lykkeMerchantId: '',
            apiKey: '',
            files: [],
            body: '',
            result: ''
        };

        vm.handlers = {
            init: init,
            submit: submit,
            addFiles: addFiles,
            getFileExtension: fileSvc.getExtension,
            getFileSize: fileSvc.getSize
        };

        function init(merchantId) {
            vm.model.lykkeMerchantId = merchantId;
        }

        function validate() {
            vm.form.errors = [];

            vm.form.errors['lykkeMerchantId'] = !vm.model.lykkeMerchantId;
            vm.form.errors['apiKey'] = !vm.model.apiKey;
            vm.form.errors['files'] = !vm.model.files.length;
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
                body: vm.model.body
            };

            apiSvc.signRequest(model, vm.model.files[0])
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

        function addFiles(files) {
            if (!files || files.length === 0)
                return;

            var valid = true;
            angular.forEach(files, function (file, key) {
                valid = valid && fileSvc.validate(file, ['pem']);
            });

            if (!valid) {
                confirmModalSvc.open({
                    title: 'Invalid file',
                    content: "Check that file is correct and has extension .pem"
                });

                return;
            }

            vm.model.files.length = 0;

            angular.forEach(files, function (file, key) {
                vm.model.files.push(file);
            });
        }
    }
})();
