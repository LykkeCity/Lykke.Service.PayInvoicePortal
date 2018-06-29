(function () {
    'use strict';

    angular
        .module('app')
        .directive('selectpicker', ['$timeout', '$parse', selectpicker]);

    function selectpicker($timeout, $parse) {
        var directive = {
            restrict: 'A',
            scope: {
                selectpickerOptions: '<?'
            },
            link: function (scope, element, attributes) {

                $timeout(function() {
                    element.selectpicker({
                        mobile: isMobile
                    });

                    //bind to collection to watch if it is changed
                    if (scope.selectpickerOptions) {
                        scope.$watch('selectpickerOptions', function(newVal, oldVal) {
                            $(element).selectpicker('refresh');
                        });
                    }
                });

                var onChangeSelectPicker = scope.$on('changeSelectPicker', function (evt, data) {
                    $timeout(function() {
                        // https://developer.snapappointments.com/bootstrap-select/methods
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
