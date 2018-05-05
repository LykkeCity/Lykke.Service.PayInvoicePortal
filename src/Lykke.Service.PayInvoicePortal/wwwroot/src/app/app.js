(function () {
    'use strict';

    angular.module('app', ['ngFileUpload'])
        .config(['$compileProvider', 'datetimepickerProvider', appConfig])
        .run(['$rootScope', startup]);

    function appConfig($compileProvider, datetimepickerProvider) {
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
})();