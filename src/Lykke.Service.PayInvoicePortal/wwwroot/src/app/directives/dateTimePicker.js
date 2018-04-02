(function () {
    'use strict';

    angular
        .module('app')
        .provider('datetimepicker', dateTimePickerProvider)
        .directive('datetimepicker', ['datetimepicker', dateTimePicker]);

    function dateTimePickerProvider() {
        var default_options = {};

        this.setOptions = function (options) {
            default_options = options;
        };

        this.$get = function () {
            return {
                getOptions: function () {
                    return default_options;
                }
            };
        };
    }

    function dateTimePicker(datetimepicker) {
        var directive = {
            require: '?ngModel',
            restrict: 'AE',
            scope: {
                datetimepickerOptions: '@'
            },
            link: function ($scope, $element, $attrs, ngModelCtrl) {

                var default_options = datetimepicker.getOptions();

                var passed_in_options = $scope.$eval($attrs.datetimepickerOptions);
                var options = jQuery.extend({}, default_options, passed_in_options);

                $element
                    .on('dp.change',
                        function (e) {
                            if (ngModelCtrl) {
                                ngModelCtrl.$setViewValue($element.data('DateTimePicker').date());
                            }
                        })
                    .datetimepicker(options);

                $scope.$watch(
                    function () {
                        return ngModelCtrl.$modelValue;
                    },
                    function (newValue) {
                        if (newValue)
                        $element
                            .data('DateTimePicker')
                            .date(newValue.toDate());
                    });
            }
        };

        return directive;
    }
})();