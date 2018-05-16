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
                    $timeout(function () {
                        var carouselData = carousel.data('owl.carousel');
                        carouselData.refresh();
                        carouselData.trigger('resized');
                    });
                });

                scope.$on('$destroy', function () {
                    if (onInvoicesLoaded) {
                        onInvoicesLoaded();
                    }
                });

                function initButtons() {
                    right.click(function (e) {
                        carousel.trigger('next.owl.carousel');
                    });

                    left.click(function (e) {
                        carousel.trigger('prev.owl.carousel');
                    });
                }

                function initTabsCarousel() {
                    carousel.owlCarousel({
                        nav: false,
                        loop: false,
                        autoWidth: true,
                        touchDrag: true,
                        mouseDrag: true,
                        items: 999999,
                        onInitialized: function () {
                            if (carousel.find('.owl-stage').width() > carousel.find('.owl-stage-outer').width()) {
                                right.addClass('invoice_tabs__arrow--in');
                            }
                            adjustWidth();
                        },
                        onResized: function (e) {
                            adjustWidth();
                            adjustArrows(e);
                        },
                        onChanged: function (e) {
                            adjustArrows(e);
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

                function adjustWidth() {
                    var owlStage = carousel.find('.owl-stage');
                    var newWidth = 0;
                    owlStage.children().toArray()
                        .forEach(function (item) {
                            newWidth += item.getBoundingClientRect().width;
                        });
                    owlStage.css('width', Math.ceil(newWidth));
                }
            }
        };

        return directive;
    }
})();
