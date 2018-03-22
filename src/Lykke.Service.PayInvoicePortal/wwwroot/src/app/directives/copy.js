(function () {
    'use strict';

    angular
        .module('app')
        .directive('copy', copy);

    function copy() {
        var directive = {
            restrict: 'A',
            scope: {
                value: '=copy'
            },
            link: function (scope, element, attributes) {
                var clipboard = new Clipboard(element[0], {
                    text: function () {
                        return scope.value;
                    }
                });

                clipboard.on('success', function (e) {
                    e.trigger.innerHTML = '<i class="icon icon--check_thin"></i> Copied';
                    e.clearSelection();
                });
            }
        };

        return directive;
    }
})();