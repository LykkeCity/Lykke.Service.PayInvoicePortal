(function () {
    'use strict';

    angular.module('app', ['ngFileUpload'])
        .config(['$compileProvider', 'datetimepickerProvider', appConfig])
        .run(['$rootScope', startup]);

    function appConfig($compileProvider, datetimepickerProvider) {
        $compileProvider.debugInfoEnabled(false);

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