(function () {
    'use strict';

    angular
        .module('app')
        .service('apiSvc', apiSvc);

    apiSvc.$inject = ['$http', '$q', '$window'];

    function apiSvc($http, $q, $window) {
        var minDate = '0001-01-01T00:00:00';

        var service = {
            subscribe: subscribe,

            getPaymentAssetsOfMerchant: getPaymentAssetsOfMerchant,

            exportToCsv: exportToCsv,

            getIncomingInvoices: getIncomingInvoices,
            getSumToPay: getSumToPay,
            payInvoices: payInvoices,
            getSupervisingInvoices: getSupervisingInvoices,
        };

        return service;

        // Subscribe

        function subscribe(model) {
            return post('subscriber', model);
        }

        // Assets

        function getPaymentAssetsOfMerchant() {
            return get("paymentAssetsOfMerchant");
        }

        // Export

        function exportToCsv(searchValue, period, status, sortField, sortAscending, isSupervising) {
            var url = isSupervising ? getUrl('export/supervising') : getUrl('export');

            url = url + "?searchValue=" + (searchValue || '');
            url = url + "&period=" + period;

            for (var i = 0; i < status.length; i++) {
                url = url + "&status=" + status[i];
            }

            url = url + "&sortField=" + (sortField || '');
            url = url + "&sortAscending=" + sortAscending;

            $window.open(url);
        }

        // Invoices

        function getIncomingInvoices(searchValue, period, statuses, skip, take) {
            return get('incomingInvoices',
                {
                    searchValue: searchValue,
                    period: period,
                    statuses: statuses,
                    skip: skip,
                    take: take
                });
        }

        function getSumToPay(invoicesIds, assetForPay) {
            return get('incomingInvoices/sum', {
                invoicesIds: invoicesIds,
                assetForPay: assetForPay
            });
        }

        function payInvoices(model) {
            return post('incomingInvoices/pay', model);
        }

        function getSupervisingInvoices(searchValue, period, status, sortField, sortAscending, skip, take) {
            return get('invoices/supervising',
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

        // Private

        function get(action, params) {
            var deferred = $q.defer();

            params = params || {};
            params['c'] = new Date().getTime();

            $http.get(getUrl(action), { params: params })
                .success(function (data, status, headers, config) {
                    deferred.resolve(data);
                })
                .error(function (data, status, headers, config) {
                    deferred.reject(data, status);
                });

            return deferred.promise;
        }

        function post(action, model) {
            var deferred = $q.defer();

            $http.post(getUrl(action), model)
                .success(function (data, status, headers, config) {
                    deferred.resolve(data);
                })
                .error(function (data, status, headers, config) {
                    deferred.reject(data, status);
                });

            return deferred.promise;
        }

        function put(action, model) {
            var deferred = $q.defer();

            $http.put(getUrl(action), model)
                .success(function (data, status, headers, config) {
                    deferred.resolve(data);
                })
                .error(function (data, status, headers, config) {
                    deferred.reject(data, status);
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
                    deferred.reject(data, status);
                });

            return deferred.promise;
        }

        function getUrl(path) {
            return '/api/' + path;
        }
    }
})();
