(function () {
    'use strict';

    angular
        .module('app')
        .directive('tabsCarousel', ['$timeout', tabsCarousel]);

    function tabsCarousel($timeout) {
        var directive = {
            restrict: 'A',
            scope: {},
            link: function (scope, element, attributes) {
                var container = $(element);
                var carousel = container.find('.invoice_tabs__carousel');
                var right = container.find('.invoice_tabs__arrow--right');
                var left = container.find('.invoice_tabs__arrow--left');

                var firstLoad = true;

                init();

                function init() {
                    initButtons();
                    $timeout(function () {
                        initTabsCarousel();
                    });
                }

                var onInvoicesLoaded = scope.$on('invoicesLoadedForCarousel', function (evt, selectedIndex) {
                    console.log('invoicesLoaded');
                    $timeout(function () {
                        // carousel.trigger('resize.owl.carousel');
                    });
                });

                scope.$on('$destroy', function () {
                    if (onInvoicesLoaded) {
                        onInvoicesLoaded();
                    }
                });

                function initButtons() {
                    right.click(function (e) {console.log('right.click')
                        carousel.trigger('next.owl.carousel');
                    });

                    left.click(function (e) {console.log('left.click')
                        carousel.trigger('prev.owl.carousel');
                    });
                }

                function initTabsCarousel() {
                    carousel.owlCarousel({
                        nav: true,
                        loop: false,
                        autoWidth: true,
                        touchDrag: true,
                        mouseDrag: true,
                        items: 999999,
                        onInitialized: function () {
                            console.log('onInitialized')
                            if (carousel.find('.owl-stage').width() > carousel.find('.owl-stage-outer').width()) {
                                console.log('onInitialized', carousel.find('.owl-stage').width(), carousel.find('.owl-stage-outer').width())
                                right.addClass('invoice_tabs__arrow--in');
                            }
                        },
                        onResized: function (e) {
                            console.log('onResized', e)
                            var owlStage = carousel.find('.owl-stage');
                            owlStage.css('width', "+=5");
                            adjustArrows(e);
                        },
                        onChanged: function (e) {
                            console.log('onChanged', 'e.page.index ' + e.page.index, 'e.page.count ' + e.page.count)
                            adjustArrows(e);
                        },
                        onDragged: function (e) {
                            console.log('onDragged');
                        }
                    });

                    function adjustArrows(e) {
                        if (e.page.index > 0) {
                            left.addClass('invoice_tabs__arrow--in');
                        } else {
                            left.removeClass('invoice_tabs__arrow--in');
                        }

                        if ((e.page.index + 1) == e.page.count) {
                            right.removeClass('invoice_tabs__arrow--in');
                        } else {
                            right.addClass('invoice_tabs__arrow--in');
                        }
                    }
                }
            }
        };

        return directive;
    }
})();
