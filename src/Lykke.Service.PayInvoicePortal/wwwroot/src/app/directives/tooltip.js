(function () {
    'use strict';

    angular
        .module('app')
        .directive('tooltip', ['$timeout', tooltip]);

    function tooltip($timeout) {
        var directive = {
            restrict: 'A',
            scope: {
                trigger: '&trigger',
                title: '@title'
            },
            link: function (scope, element, attributes) {
                var timeout;
                var tooltip = $(element).tooltip({ 'title': scope.title, 'placement': 'top', 'trigger': 'manual' });

                scope.trigger({
                    handler: function() {
                        tooltip.tooltip('show');
                        if (angular.isDefined(timeout)) {
                            $timeout.cancel(timeout);
                        }
                        timeout = $timeout(function () {
                            tooltip.tooltip('hide');
                        }, 2000);
                    }
                });

                scope.$on('$destroy', function () {
                    if (angular.isDefined(timeout)) {
                        $timeout.cancel(timeout);
                    }
                    tooltip.tooltip('destroy');
                    scope.trigger({
                        handler: undefined
                    });
                });
            }
        };

        return directive;
    }
})();