(function () {
    'use strict';

    angular
        .module('app')
        .service('fileSvc', fileSvc);

    fileSvc.$inject = [];

    function fileSvc() {
        var service = {
            getExtension: getExtension,
            getSize: getSize,
            validate: validate,
            getError: getError
        };

        return service;

        function getExtension(fileName) {
            if (fileName)
                return fileName.split('.').pop();

            return '';
        }

        function getSize(value) {
            if (value < 1024) {
                return value + ' bytes';
            } else if (value > 1024 && value < 1048576) {
                return (value / 1024).toFixed(0) + ' KB';
            } else {
                return (value / 1048576).toFixed(0) + ' MB';
            }
        }

        function validate(file) {
            if (!file)
                return false;

            if (file.size > 5242880)
                return false;

            var extension = getExtension(file.name);

            if (!extension)
                return false;

            return ['jpg', 'jpeg', 'png','pdf', 'doc', 'docx', 'xls', 'xlsx', 'rtf'].indexOf(extension.toLowerCase()) >= 0;
        }

        function getError() {
            return 'One or more files are invalid. Please check the requirements: <ul><li>• the maximum file size is 5 MB</li><li>• allowed types: .jpg; .jpeg; .pdf; .doc; .docx; .xls; .xlsx.; .rtf</li></ul>';
        }
    }
})();