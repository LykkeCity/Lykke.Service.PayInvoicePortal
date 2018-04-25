(function () {
    'use strict';

    angular
        .module('app')
        .directive('clickOutside', [
            '$document', '$parse', '$timeout', 'confirmModalSvc',
            clickOutside
        ]);

    function clickOutside($document, $parse, $timeout, confirmModalSvc) {
        return {
            restrict: 'A',
            link: function ($scope, elem, attr) {

                $timeout(function () {
                    var classList = (attr.exclude !== undefined) ? attr.exclude.split(/[ ,]+/) : [],
                        fn;

                    confirmModalSvc.excludeClickClassList.forEach(function(item) {
                        classList.push(item);
                    });

                    function eventHandler(e) {
                        var i,
                            element,
                            r,
                            id,
                            classNames,
                            l;

                        if (angular.element(elem).hasClass("ng-hide")) {
                            return;
                        }

                        if (!e || !e.target) {
                            return;
                        }

                        if (e.target.type === 'file' || e.target.type === 'button')
                            return;

                        for (element = e.target; element; element = element.parentNode) {
                            if (element === elem[0]) {
                                return;
                            }

                            id = element.id,
                                classNames = element.className,
                                l = classList.length;

                            if (classNames && classNames.baseVal !== undefined) {
                                classNames = classNames.baseVal;
                            }

                            if (classNames || id) {
                                for (i = 0; i < l; i++) {
                                    r = new RegExp('\\b' + classList[i] + '\\b');
                                    if ((id !== undefined && id === classList[i]) || (classNames && r.test(classNames)))
                                        return;
                                }
                            }
                        }

                        $timeout(function () {
                            fn = $parse(attr['clickOutside']);
                            fn($scope, { event: e });
                        });
                    }

                    if (hasTouch()) {
                        $document.on('touchstart', eventHandler);
                    }

                    $document.on('click', eventHandler);

                    $scope.$on('$destroy', function () {
                        if (hasTouch()) {
                            $document.off('touchstart', eventHandler);
                        }

                        $document.off('click', eventHandler);
                    });

                    function hasTouch() {
                        return 'ontouchstart' in window || navigator.maxTouchPoints;
                    };
                });
            }
        };
    }
})();
