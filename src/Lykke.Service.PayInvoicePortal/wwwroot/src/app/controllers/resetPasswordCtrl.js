(function () {
    'use strict';

    angular
        .module('app')
        .controller('resetPasswordCtrl', resetPasswordCtrl);

    resetPasswordCtrl.$inject = ['$log', '$rootScope', 'apiSvc', 'confirmModalSvc'];

    function resetPasswordCtrl($log, $rootScope, apiSvc, confirmModalSvc) {
        var vm = this;

        var token = '';

        vm.view = {
            isVisiblePassword: false,
            isVisibleReenterPassword: false,
            isLoading: false
        };

        vm.form = {
            errors: []
        };

        vm.model = {
            password: '',
            reenterPassword: ''
        };

        vm.handlers = {
            init: init,
            toggleVisibilityPassword: toggleVisibilityPassword,
            toggleVisibilityReenterPassword: toggleVisibilityReenterPassword,
            validatePassword: validatePassword,
            validateReenterPassword: validateReenterPassword,
            submit: submit
        };

        function init(data) {
            token = data.token;
        }

        function toggleVisibilityPassword() {
            vm.view.isVisiblePassword = !vm.view.isVisiblePassword;
        }

        function toggleVisibilityReenterPassword() {
            vm.view.isVisibleReenterPassword = !vm.view.isVisibleReenterPassword;
        }

        function validatePassword() {
            vm.form.errors.password = !vm.model.password;
        }

        function validateReenterPassword() {
            vm.form.errors.reenterPassword = !vm.model.reenterPassword;
            vm.form.errors.reenterPasswordNotEqual = vm.model.reenterPassword && vm.model.password !== vm.model.reenterPassword;
        }

        function validate() {
            vm.form.errors = [];

            validatePassword();
            validateReenterPassword();

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

            var model = {
                token: token,
                password: vm.model.password
            };

            apiSvc.resetPassword(model)
                .then(
                    function () {
                        // redirect to Sing in page
                        window.location.href = '/welcome'
                    },
                    function (error) {
                        $log.error(error);

                        var message = error.errorMessage === 'Invalid token' ? 'Token is invalid or expired.' : 'Unable to change password.'

                        confirmModalSvc.open({
                            title: confirmModalSvc.constants.errorTitle,
                            content: message + ' Please contact support.'
                        });
                    })
                .finally(
                    function () {
                        vm.view.isLoading = false;
                    });
        }
    }
})();
