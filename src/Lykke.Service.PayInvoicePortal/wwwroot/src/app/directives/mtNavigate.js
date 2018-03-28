(function () {
    'use strict';

    angular
        .module('app')
        .directive('mtNavigate', mtNavigate);

    function mtNavigate() {
        var directive = {
            restrict: 'A',
            scope: {
                action: '&mtNavigate'
            },
            link: function (scope, element, attributes) {
                element.bind('click', function (event) {
                    var target = event.target;

                    while (target.nodeName !== 'TD') {
                        target = target.parentElement;
                    }

                    if ($(target).find(':button').length === 0) {
                        scope.action();
                    }
                });
            }
        };

        return directive;
    }
})();