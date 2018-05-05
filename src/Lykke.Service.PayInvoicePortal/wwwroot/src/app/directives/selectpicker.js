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
                
            }
        };

        return directive;
    }
})();
