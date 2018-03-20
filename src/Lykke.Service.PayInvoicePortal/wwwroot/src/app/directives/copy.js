(function () {
    'use strict';

    angular
        .module('app')
        .directive('copy', copy);

    function copy() {
        var directive = {
            restrict: 'A',
            scope: {
                value: '=copy',
                title: '@copyTitle'
            },
            link: function (scope, element, attributes) {
                $(element).tooltip({
                    show: {
                        effect: "slideDown",
                        delay: 250
                    }
                });

                element.bind('click', function (event) {
                    event.stopPropagation();
                    var $temp = $("<input>");
                    $("body").append($temp);
                    $temp.val(scope.value).select();
                    document.execCommand("copy");
                    $temp.remove();

                    $(element).tooltip('hide').attr('data-original-title', scope.title).tooltip('show');
                });
            }
        };

        return directive;
    }
})();