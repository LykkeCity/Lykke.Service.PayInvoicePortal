(function () {
    'use strict';

    angular.module('app', ['ngFileUpload'])
        .factory('authHttpResponseInterceptor',['$q','$location', '$window', authHttpResponseInterceptor])
        .config(['$compileProvider', '$httpProvider', 'datetimepickerProvider', appConfig])
        .run(['$rootScope', startup]);

    function appConfig($compileProvider, $httpProvider, datetimepickerProvider) {
        $httpProvider.interceptors.push('authHttpResponseInterceptor');

        var debugEnabled = window.location.host.indexOf('localhost') > -1 || window.location.host.indexOf('dev') > -1;
        $compileProvider.debugInfoEnabled(debugEnabled);

        datetimepickerProvider.setOptions({
            format: 'l',
            minDate: moment().startOf("day"),
            icons: {
                time: 'icon--clock',
                date: "icon--cal",
                up: "icon--chevron-thin-up",
                down: "icon--chevron-thin-down",
                previous: "icon--chevron-thin-left",
                next: "icon--chevron-thin-right"
            }
        });
    }

    function startup($rootScope) {
        $rootScope.blur = false;
    }


    function authHttpResponseInterceptor($q, $location, $window){
        return {
            responseError: function(rejection) {
                if (rejection.status === 401) {
                    var path = $location.path();
                    $window.location.href = '/welcome' + path && path.length ? '?ReturnUrl=' + path : '';
                }
                return $q.reject(rejection);
            }
        }
    }
})();
