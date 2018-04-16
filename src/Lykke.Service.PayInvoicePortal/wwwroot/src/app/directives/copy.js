(function () {
    'use strict';

    angular
        .module('app')
        .directive('copy', ['$timeout', copy]);

    function copy($timeout) {
        var directive = {
            restrict: 'A',
            scope: {
                value: '=copy',
                title: '@title'
            },
            link: function (scope, element, attributes) {
                var clipboard = new Clipboard(element[0], {
                    text: function () {
                        return scope.value;
                    }
                });

                var timeout;
                var tooltip = $(element).tooltip({ 'title': scope.title, 'placement': 'top', 'trigger': 'manual' });
                
                clipboard.on('success', function (e) {
                    e.clearSelection();
                    tooltip.tooltip('show');

                    if (angular.isDefined(timeout)) {
                        $timeout.cancel(timeout);
                    }
                    timeout = $timeout(function() {
                        tooltip.tooltip('hide');
                    }, 2000);
                });

                scope.$on('$destroy', function () {
                    if (angular.isDefined(timeout)) {
                        $timeout.cancel(timeout);
                    }
                    clipboard.destroy();
                    tooltip.tooltip('destroy');
                });
            }
        };

        return directive;
    }
})();