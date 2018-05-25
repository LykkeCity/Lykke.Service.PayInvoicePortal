(function() {
    'use strict';

    angular.module('app').filter('nzcurrency', ['$filter', nzcurrency]);

    function nzcurrency($filter) {
        var currency = $filter('currency');

        return function(amount, accuracy) {
            if (typeof(amount) === 'undefined') {
                return '';
            }

            var value = currency(amount, '', accuracy);

            var result = value.replace(/0+$/, '').replace(/\.+$/, '');

            return result;
        };
    }
})();
