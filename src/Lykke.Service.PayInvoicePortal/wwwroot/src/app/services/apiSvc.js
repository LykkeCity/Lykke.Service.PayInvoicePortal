(function () {
    'use strict';

    angular
        .module('app')
        .service('apiSvc', apiSvc);

    apiSvc.$inject = ['$http', '$q', '$window', 'Upload'];

    function apiSvc($http, $q, $window, Upload) {
        var minDate = '0001-01-01T00:00:00';

        var service = {
            subscribe: subscribe,

            sendEmail: sendEmail,

            getAssets: getAssets,
            getPaymentAssets: getPaymentAssets,

            getBalance: getBalance,

            exportToCsv: exportToCsv,

            uploadFile: uploadFile,
            getFile: getFile,
            deleteFile: deleteFile,

            getInvoice: getInvoice,
            getInvoices: getInvoices,
            getSupervisingInvoices: getSupervisingInvoices,
            saveInvoice: saveInvoice,
            updateInvoice: updateInvoice,
            deleteInvoice: deleteInvoice,

            getPaymentDetails: getPaymentDetails,
            refreshPaymentDetails: refreshPaymentDetails,
            getPaymentStatus: getPaymentStatus,
            changePaymentAsset: changePaymentAsset
        };

        return service;

        // Subscribe

        function subscribe(model) {
            return post('subscriber', model);
        }

        // Email

        function sendEmail(model) {
            return post('email', model);
        }

        // Assets

        function getAssets() {
            return get('assets', {});
        }

        function getPaymentAssets(merchantId, settlementAssetId) {
            return get("paymentAssets", {
                merchantId: merchantId,
                settlementAssetId: settlementAssetId
            });
        }

        // Balances

        function getBalance() {
            return get('balances', {});
        }

        // Export

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

        // Files

        function uploadFile(invoiceId, files) {
            return upload('files', { invoiceId: invoiceId }, files);
        }

        function getFile(invoiceId, fileId) {
            $window.open(getUrl('files') + '/' + fileId + '?invoiceId=' + invoiceId);
        }

        function deleteFile(invoiceId, fileId) {
            return remove('files/' + fileId, { invoiceId: invoiceId });
        }

        // Invoices

        function getInvoice(invoiceId) {
            return get('invoices/' + invoiceId);
        }

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

        function saveInvoice(model, files) {
            return upload('invoices', model, files);
        }

        function updateInvoice(model) {
            return put('invoices', model);
        }

        function deleteInvoice(invoiceId) {
            return remove('invoices/' + invoiceId, {});
        }

        // Paymnets

        function getPaymentDetails(invoiceId) {
            return get('payments/' + invoiceId, {});
        }

        function refreshPaymentDetails(invoiceId) {
            return get('payments/refresh/' + invoiceId, {});
        }

        function getPaymentStatus(invoiceId) {
            return get('payments/' + invoiceId + '/status', {});
        }

        function changePaymentAsset(invoiceId, paymentAssetId) {
            return post('payments/changeasset/' + invoiceId + '/' + paymentAssetId)
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
                    deferred.reject('Error: ' + status);
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
                    deferred.reject('Error: ' + status);
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
