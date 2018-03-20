(function () {
    'use strict';

    angular
        .module('app')
        .service('apiSvc', apiSvc);

    apiSvc.$inject = ['$http', '$q', '$window', 'Upload'];

    function apiSvc($http, $q, $window, Upload) {
        var minDate = '0001-01-01T00:00:00';

        var service = {
            getInvoices: getInvoices,
            saveInvoice: saveInvoice,
            removeInvoice: removeInvoice,
            exportToCsv: exportToCsv,
            getBalance: getBalance,
            getAssets: getAssets
        };

        return service;

        function getInvoices(searchValue, period, status, sortField, sortAscending, skip, take) {
            return get('invoices',
                {
                    searchValue: searchValue,
                    period: period,
                    status: status,
                    sortField: sortField,
                    sortAscending: sortAscending,
                    skip: skip,
                    take: take
                });
        }

        function saveInvoice(model, files) {
            return upload('invoices', model, files);
        }
        
        function removeInvoice(invoiceId) {
            return remove('invoices', { invoiceId: invoiceId });
        }

        function exportToCsv(searchValue, period, status, sortField, sortAscending) {
            var url = getUrl('export');

            url = url + "?searchValue=" + (searchValue || '');
            url = url + "&period=" + period;

            for (var i = 0; i < status.length; i++) {
                url = url + "&status=" + status[i];
            }

            url = url + "&sortField=" + (sortField || '');
            url = url + "&sortAscending=" + sortAscending;

            $window.open(url);
        }

        function getBalance() {
            return get('balances', {});
        }

        function getAssets() {
            return get('assets', {});
        }

        function get(action, params) {
            var deferred = $q.defer();

            params = params || {};
            params['c'] = new Date().getTime();

            $http.get(getUrl(action), { params: params })
                .success(function (data, status, headers, config) {
                    deferred.resolve(data);
                })
                .error(function (data, status, headers, config) {
                    deferred.reject('Error: ' + status);
                });

            return deferred.promise;
        }

        function post(action, model) {
            var deferred = $q.defer();

            $http.post(getUrl(action), model)
                .success(function (data, status, headers, config) {
                    if (data.HasErrors === false) {
                        deferred.resolve(data.Value);
                    } else {
                        deferred.reject(data.ErrorMessage);
                    }
                })
                .error(function (data, status, headers, config) {
                    deferred.reject('Error: ' + status);
                });

            return deferred.promise;
        }

        function remove(action, params) {
            var deferred = $q.defer();

            params = params || {};
            params['c'] = new Date().getTime();

            $http.delete(getUrl(action), { params: params })
                .success(function (data, status, headers, config) {
                    deferred.resolve(data);
                })
                .error(function (data, status, headers, config) {
                    deferred.reject('Error: ' + status);
                });

            return deferred.promise;
        }

        function upload(action, model, files) {
            var deferred = $q.defer();

            Upload.upload({
                url: getUrl(action),
                file: files,
                fileFormDataName: 'files',
                fields: model
            }).then(
                function(response) {
                    deferred.resolve(response.data);
                },
                function (response) {
                    deferred.reject(response);
                },
                function (evt) {
                });

            return deferred.promise;
        }

        function getUrl(path) {
            return '/api/' + path;
        }
    }
})();