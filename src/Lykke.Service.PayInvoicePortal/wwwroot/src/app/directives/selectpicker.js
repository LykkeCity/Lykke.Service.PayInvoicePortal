(function () {
    'use strict';

    angular
        .module('app')
        .directive('selectpicker', ['$timeout', selectpicker]);

    function selectpicker($timeout) {
        var directive = {
            restrict: 'A',
            scope: {},
            link: function (scope, element, attributes) {

                $timeout(function() {
                    element.selectpicker({
                        mobile: isMobile
                    });
                });

                var onChangeSelectPicker = scope.$on('changeSelectPicker', function (evt, data) {
                    $timeout(function() {
                        // https://silviomoreto.github.io/bootstrap-select/methods/
                        $(element).selectpicker('refresh');
                    });
                });

                scope.$on('$destroy', function () {
                    if (onChangeSelectPicker) {
                        onChangeSelectPicker();
                    }
                });
            }
        };

        return directive;
    }
})();
