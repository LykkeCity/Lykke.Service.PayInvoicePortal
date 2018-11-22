(function () {
    'use strict';

    angular.module('app', [])
        .factory('authHttpResponseInterceptor',['$q','$location', '$window', authHttpResponseInterceptor])
        .config(['$compileProvider', '$httpProvider', appConfig])
        .run(['$rootScope', startup]);

    function appConfig($compileProvider, $httpProvider) {
        $httpProvider.interceptors.push('authHttpResponseInterceptor');

        var debugEnabled = window.location.host.indexOf('localhost') > -1 || window.location.host.indexOf('dev') > -1;
        $compileProvider.debugInfoEnabled(debugEnabled);
    }

    function startup($rootScope) {
        $rootScope.blur = false;
    }


    function authHttpResponseInterceptor($q, $location, $window){
        return {
            responseError: function(rejection) {
                if (rejection.status === 401) {
                    var path = $location.path();
                    $window.location.href = '/auth/signin' + path && path.length ? '?ReturnUrl=' + path : '';
                }
                return $q.reject(rejection);
            }
        }
    }
})();
