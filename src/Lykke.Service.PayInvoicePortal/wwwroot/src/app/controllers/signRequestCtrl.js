(function () {
    'use strict';

    angular
        .module('app')
        .controller('signRequestCtrl', signRequestCtrl);

    signRequestCtrl.$inject = ['$scope', '$window', '$log', '$rootScope', '$interval', 'apiSvc', 'statusSvc', 'fileSvc', 'confirmModalSvc'];

    function signRequestCtrl($scope, $window, $log, $rootScope, apiSvc, fileSvc, confirmModalSvc) {
        var vm = this;

        vm.view = {
            isLoading: false
        };

        vm.model = {
            lykkeMerchantId: '',
            apiKey: '',
            files: [],
            body: '',
            result: ''
        };

        vm.handlers = {
            submit: submit,
            addFiles: addFiles,
            getFileExtension: fileSvc.getExtension,
            getFileSize: fileSvc.getSize,
            getFile: getFile
        };

        activate();

        function activate() {
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

            var model =
                {
                    lykkeMerchantId: vm.model.lykkeMerchantId,
                    apiKey: vm.model.apiKey,
                    body: vm.model.body
                };

            apiSvc.signRequest(model, vm.model.files)
                .then(
                    function (data) {
                        vm.model.result = data.signedBody;
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
                        vm.view.isLoading = false;
                    });
        }
    }
})();
