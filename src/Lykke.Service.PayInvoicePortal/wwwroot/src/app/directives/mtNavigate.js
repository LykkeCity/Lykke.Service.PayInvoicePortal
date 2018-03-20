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
                element.attr('style', 'cursor: pointer;');
                element.bind('click', function (event) {
                    if (event.target.nodeName === 'TD' && $(event.target).find(':input').length === 0) {
                        scope.action();
                    }
                });
            }
        };

        return directive;
    }
})();