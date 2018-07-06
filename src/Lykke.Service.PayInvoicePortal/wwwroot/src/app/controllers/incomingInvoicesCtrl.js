(function () {
    'use strict';

    angular
        .module('app')
        .controller('incomingInvoicesCtrl', incomingInvoicesCtrl);

        incomingInvoicesCtrl.$inject = ['$scope', '$window', '$rootScope', '$interval', '$log', 'apiSvc', 'statusSvc', 'confirmModalSvc'];

    function incomingInvoicesCtrl($scope, $window, $rootScope, $interval, $log, apiSvc, statusSvc, confirmModalSvc) {
        var vm = this;

        var calculatedSumToPay = 0;
        var assetForPayLsKey = 'incomingInvoices_assetForPay';
        var emptySelectSource = [{id:'', title:'Nothing selected'}];

        vm.events = {
            invoicesPaid: undefined
        };

        vm.model = {
            paymentAssets: emptySelectSource,
            baseAsset: '',
            invoices: [],
            selectedInvoices: [],
            sumToPay: 0,
            assetForPay: ''
        };

        vm.filter = {
            status: '',
            period: 0,
            search: '',
            total: 0,
            statuses: [
                { id: '', title: 'All', values: null, count: 0 },
                { id: 'unpaid', title: 'Unpaid', values: ['Unpaid'], count: 0 },
                { id: 'inProgress', title: 'In Progress', values: ['InProgress'], count: 0 },
                { id: 'paid', title: 'Paid', values: ['Paid'], count: 0 },
                { id: 'underpaid', title: 'Underpaid', values: ['Underpaid'], count: 0 },
                { id: 'overpaid', title: 'Overpaid', values: ['Overpaid'], count: 0 },
                { id: 'latePaid', title: 'LatePaid', values: ['LatePaid'], count: 0 },
                { id: 'pastDue', title: 'PastDue', values: ['PastDue'], count: 0 },
                { id: 'error', title: 'Error', values: ['InternalError', 'InvalidAddress', 'NotConfirmed'], count: 0 }
            ],
            periods: [
                { id: 0, title: 'All time' },
                { id: 2, title: 'Last month' },
                { id: 3, title: 'Three month ago' }
            ],
            handlers: {
                clearSearch: clearSearch
            }
        };

        vm.pager = {
            pageSize: 20,
            page: 1
        };

        vm.handlers = {
            getStatusCss: statusSvc.getStatusCss,
            showMore: showMore,
            onSelected: onSelected,
            onAssetChange: onAssetChange,
            payInvoices: payInvoices
        };

        vm.view = {
            isFirstLoading: true,
            hasInvoices: true,
            isLoading: false,
            get showNoResults() {
                return showNoResults();
            },
            get canShowMore() {
                return canShowMore();
            },
            isPayLoading: false,
            get isPayDisabled() {
                return vm.view.isPayLoading
                    || !vm.model.assetForPay
                    || !(vm.model.sumToPay > 0 && vm.model.sumToPay <= calculatedSumToPay);
            },
            canBeSelected: canBeSelected
        };

        activate();

        function activate() {
            apiSvc.getPaymentAssetsOfMerchant()
                .then(
                    function(data) {
                        if (data && data.length)
                            vm.model.paymentAssets = data;
                    },
                    function(error) {
                        $log.error(error);
                    }
                );

            loadInvoices();

            $scope.$watch(
                function () { return vm.filter.search; },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        applyFilter();
                    }
                }
            );

            $scope.$watch(
                function () { return vm.filter.period; },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        applyFilter();
                    }
                }
            );

            $scope.$watch(
                function () { return vm.filter.status; },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        applyFilter();
                    }
                }
            );

            $scope.$on('$destroy',
                function () {
                    destroy();
                });

            vm.events.invoicesPaid = $scope.$on('invoicesPaid', function (evt, data) {
                vm.view.isLoading = true;
                loadInvoices().finally(function() {
                    vm.view.isLoading = false;
                });
            });

            $interval(updateInvoicesByInterval, 5 * 60 * 1000);

            function applyFilter() {
                vm.view.isLoading = true;
                resetPage();
                loadInvoices().finally(function() {
                    vm.view.isLoading = false;
                });
            }
        }

        function destroy() {
            if (vm.events.invoicesPaid)
                vm.events.invoicesPaid();
        }

        function updateInvoicesByInterval() {
            loadInvoices();
        }

        function loadInvoices() {
            return apiSvc.getIncomingInvoices(vm.filter.search,
                    vm.filter.period,
                    getFilterStatuses(),
                    0,
                    vm.pager.pageSize * vm.pager.page)
                .then(
                    function(data) {
                        updateData(data);
                    },
                    function(error) {
                        $log.error(error);
                    });
        }

        function reloadInvoices() {
            vm.view.isLoading = true;
            loadInvoices().finally(function() {
                vm.view.isLoading = false;
            });
        }

        function updateData(data) {
            updateList(data.list);

            vm.model.baseAsset = data.baseAsset || '';

            //set assetForPay
            var savedAssetForPay = localStorage.getItem(assetForPayLsKey);

            if (savedAssetForPay && vm.model.paymentAssets.filter(function(item) { return item.id === savedAssetForPay; }).length) {
                vm.model.assetForPay = savedAssetForPay;
            } else if (vm.model.paymentAssets.length) {
                vm.model.assetForPay = data.baseAsset || vm.model.paymentAssets[0].id;
            }
            $rootScope.$broadcast('changeSelectPicker');

            if (vm.view.isFirstLoading){
                vm.view.hasInvoices = data.list.items.length ? true : false;
                vm.view.isFirstLoading = false;
            }
        }

        function updateList(data) {
            vm.filter.total = data.total;

            angular.forEach(data.items, function (item, key) {
                item.dueDate = $window.moment(item.dueDate);
                item.createdDate = $window.moment(item.createdDate);
            });

            vm.model.invoices = data.items;

            // select previously selected items
            if (vm.model.selectedInvoices.length) {
                var selectedInvoicesDictionary = {};

                var selectedInvoicesLength = vm.model.selectedInvoices.length;
                for (var i = 0; i < selectedInvoicesLength; i++) {
                    selectedInvoicesDictionary[vm.model.selectedInvoices[i]] = true;
                }

                var hasFoundUnselection = false;
                for (var i = 0, len = vm.model.invoices.length; i < len; i++) {
                    var invoice = vm.model.invoices[i];

                    // check whether invoice was selected
                    if (selectedInvoicesDictionary[invoice.id]) {
                        if (canBeSelected(invoice.status)) {
                            invoice.isSelected = true;
                        } else {
                            // mark to remove invoice from selected because status was changed
                            hasFoundUnselection = true;
                            selectedInvoicesDictionary[invoice.id] = false;
                        }
                    }
                }

                if (hasFoundUnselection) {
                    vm.model.selectedInvoices = vm.model.selectedInvoices.filter(function (invoiceId) {
                        return selectedInvoicesDictionary[invoiceId];
                    });

                    getSumToPay();
                }
            }

            angular.forEach(vm.filter.statuses, function (status, key) {
                status.count = 0;
            });

            angular.forEach(vm.filter.statuses, function (status, key) {
                angular.forEach(status.values, function (value, key) {
                    var count = data.countPerStatus[value];
                    if (count)
                        status.count += count;
                });
            });
        }

        function clearSearch() {
            vm.filter.search = '';
        }

        function getFilterStatuses() {
            var statuses = [];

            if (vm.filter.status) {
                statuses = vm.filter.statuses.filter(function (status) {
                    return status.id === vm.filter.status;
                })[0].values;
            }

            return statuses;
        }

        function resetPage() {
            vm.pager.page = 1;
        }

        function canShowMore() {
            var size = vm.pager.page * vm.pager.pageSize;
            var count = 0;
            if (vm.filter.status) {
                count = vm.filter.statuses.filter(function(status) {
                    return status.id === vm.filter.status;
                })[0].count;
            } else {
                count = vm.filter.total;
            }

            return count > size;
        }

        function showMore() {
            vm.pager.page++;
            loadInvoices();
        }

        function showNoResults() {
            return vm.filter.search && !vm.model.invoices.length;
        }

        function onSelected(invoiceId, isSelected) {

            if (isSelected) {
                vm.model.selectedInvoices.push(invoiceId);
            } else {
                var index = vm.model.selectedInvoices.indexOf(invoiceId);
                if (index > -1) {
                    vm.model.selectedInvoices.splice(index, 1);
                }
            }

            if (vm.model.selectedInvoices.length) {
                getSumToPay();
            }
        }

        function payInvoices() {
            if (vm.model.assetForPay && !vm.view.isPayDisabled) {
                vm.view.isPayLoading = true;

                var model = {
                    invoicesIds: vm.model.selectedInvoices,
                    amount: vm.model.sumToPay,
                    assetForPay: vm.model.assetForPay
                };

                apiSvc.payInvoices(model)
                    .then(
                        function() {
                            // clear and reload
                            vm.model.selectedInvoices.length = 0;

                            for (var i = 0, len = vm.model.invoices.length; i < len; i++) {
                                vm.model.invoices[i].isSelected = false;
                            }

                            reloadInvoices();
                        },
                        function(error, status) {
                            $log.error(error);
                            var message = error.errorMessage || 'Internal error, please contact support';

                            confirmModalSvc.open({
                                title: 'Error occured',
                                content: message
                            });
                        }
                    )
                    .finally(function() {
                        vm.view.isPayLoading = false;
                        reloadInvoices();
                    });
            }
        }

        function onAssetChange() {
            localStorage.setItem(assetForPayLsKey, vm.model.assetForPay);

            getSumToPay();
        }

        function getSumToPay() {
            if (vm.model.assetForPay && vm.model.selectedInvoices.length) {
                vm.view.isPayLoading = true;

                apiSvc.getSumToPay(vm.model.selectedInvoices, vm.model.assetForPay)
                .then(
                    function(sumToPay) {
                        calculatedSumToPay = sumToPay;
                        vm.model.sumToPay = sumToPay;
                    },
                    function(error, status) {
                        $log.error(error);
                        var message = error.errorMessage || 'Please try again or contact support';

                        confirmModalSvc.open({
                            title: 'Error occured',
                            content: message
                        });
                    }
                )
                .finally(function() {
                    vm.view.isPayLoading = false;
                });
            }
        }

        function canBeSelected(invoiceStatus) {
            return (invoiceStatus === 'Unpaid' || invoiceStatus === 'Underpaid')
        }
    }
})();
