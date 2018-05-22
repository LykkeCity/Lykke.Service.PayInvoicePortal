(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoicesCtrl', invoicesCtrl);

    invoicesCtrl.$inject = ['$scope', '$window', '$rootScope', '$interval', '$log', 'apiSvc', 'statusSvc', 'confirmModalSvc'];

    function invoicesCtrl($scope, $window, $rootScope, $interval, $log, apiSvc, statusSvc, confirmModalSvc) {
        var vm = this;

        vm.events = {
            invoiceGenerated: undefined,
            invoiceDraftCreated: undefined
        };

        vm.model = {
            balance: 0,
            baseAsset: null,
            baseAssetAccuracy: 0,
            invoices: []
        };

        vm.statistic = {
            main: null
        };

        vm.filter = {
            show: false,
            status: null,
            period: 0,
            search: '',
            sortField: null,
            sortAscending: null,
            total: 0,
            statuses: [
                { id: 'draft', title: 'Draft', values: ['Draft'], count: 0 },
                { id: 'unpaid', title: 'Unpaid', values: ['Unpaid'], count: 0 },
                {
                    id: 'inProgress',
                    title: 'In Progress',
                    values: ['InProgress', 'RefundInProgress'],
                    count: 0
                },
                { id: 'paid', title: 'Paid', values: ['Paid'], count: 0 },
                { id: 'underpaid', title: 'Underpaid', values: ['Underpaid'], count: 0 },
                { id: 'overpaid', title: 'Overpaid', values: ['Overpaid'], count: 0 },
                { id: 'latePaid', title: 'LatePaid', values: ['LatePaid'], count: 0 },
                { id: 'refunded', title: 'Refunded', values: ['Refunded'], count: 0 },
                { id: 'pastDue', title: 'PastDue', values: ['PastDue'], count: 0 },
                { id: 'removed', title: 'Removed', values: ['Removed'], count: 0 },
                { id: 'error', title: 'Error', values: ['InternalError', 'InvalidAddress', 'NotConfirmed'], count: 0 }
            ],
            periods: [
                { id: 0, title: 'All time' },
                { id: 1, title: 'Current month' },
                { id: 2, title: 'Last month' },
                { id: 3, title: 'Three month ago' }
            ],
            handlers: {
                toggle: toggleFilter,
                close: closeFilter,
                clear: clearFilter,
                clearStatus: clearStatusFilter,
                selectStatus: selectStatusFilter
            }
        };

        vm.pager = {
            pageSize: 20,
            page: 1
        };

        vm.handlers = {
            init: init,
            exportToCsv: exportToCsv,
            getStatusCss: statusSvc.getStatusCss,
            canRemove: canRemove,
            remove: remove,
            showMore: showMore,
            canShowMore: canShowMore,
            openDetails: openDetails,
            sort: sort,
            create: create
        };

        vm.view = {
            isSearching: false,
            get showNoResults() {
                return showNoResults();
            },
            hasInvoices: true
        };

        activate();

        function activate() {
            $scope.$watch(
                function () { return vm.filter.search; },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        vm.view.isSearching = true;
                        resetPage();
                        loadInvocies().finally(function() {
                            vm.view.isSearching = false;
                        });
                    }
                }
            );
            $scope.$watch(
                function () { return vm.filter.period; },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        resetPage();
                        loadInvocies();
                    }
                }
            );
            $scope.$watch(
                function () { return vm.filter.status; },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        loadInvocies();
                    }
                }
            );

            $scope.$on('$destroy',
                function () {
                    destroy();
                });

            vm.events.invoiceGenerated = $scope.$on('invoiceGenerated', function (evt, data) {
                loadInvocies();
                vm.view.hasInvoices = true;
            });

            vm.events.invoiceDraftCreated = $scope.$on('invoiceDraftCreated', function (evt, data) {
                loadInvocies();
                vm.view.hasInvoices = true;
            });

            $interval(loadInvocies, 5 * 60 * 1000);
        }

        function init(data) {
            vm.view.hasInvoices = data.list.items.length ? true : false;
            updateData(data);
        }

        function destroy() {
            if (vm.events.invoiceGenerated)
                vm.events.invoiceGenerated();

            if (vm.events.invoiceDraftCreated)
                vm.events.invoiceDraftCreated();
        }

        function loadInvocies() {
            return apiSvc.getInvoices(vm.filter.search,
                    vm.filter.period,
                    getFilterStatuses(),
                    vm.filter.sortField,
                    vm.filter.sortAscending,
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

        function updateData(data) {
            updateList(data.list);
            vm.model.balance = data.balance;
            vm.model.baseAsset = data.baseAsset;
            vm.model.baseAssetAccuracy = data.baseAssetAccuracy;
            vm.statistic.main = data.statistic.mainStatistic;
        }

        function updateList(data) {
            vm.filter.total = data.total;

            angular.forEach(data.items, function (item, key) {
                item.dueDate = $window.moment(item.dueDate);
                item.createdDate = $window.moment(item.createdDate);
            });

            vm.model.invoices = data.items;

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

            var selectedIndex = 0;
            if (vm.filter.status) {
                var selectedStatus = vm.filter.statuses.filter(function (status, index) {
                    if (status.id === vm.filter.status) {
                        selectedIndex = index;
                        return true;
                    }
                    return false;
                })[0];

                if (selectedStatus.count === 0)
                    vm.filter.status = null;
            }

            $rootScope.$broadcast('invoicesLoadedForCarousel', selectedIndex);
        }

        function exportToCsv() {
            apiSvc.exportToCsv(vm.filter.search,
                vm.filter.period,
                getFilterStatuses(),
                vm.filter.sortField,
                vm.filter.sortAscending,
                0);
        }

        function resetFilter() {
            vm.filter.search = '';
            vm.filter.period = 0;
        }

        function toggleFilter() {
            vm.filter.show = !vm.filter.show;
            resetFilter();
        }

        function closeFilter() {
            vm.filter.show = false;
            resetFilter();
        }

        function clearFilter() {
            vm.filter.search = '';
        }

        function clearStatusFilter() {
            vm.filter.status = null;
        }

        function selectStatusFilter(status) {
            vm.filter.status = status.id;
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

        function sort(sortField) {
            if (vm.filter.sortField === sortField)
                vm.filter.sortAscending = !vm.filter.sortAscending;
            else
                vm.filter.sortAscending = true;

            vm.filter.sortField = sortField;
            loadInvocies();
        }

        function canRemove(invoice) {
            return invoice && (invoice.status === 'Draft' || invoice.status === 'Unpaid');
        }

        function remove(invoice) {
            if (!canRemove(invoice))
                return;

            confirmModalSvc.open({
                content: 'Are you sure you want to remove this invoice "#' + invoice.number + '"?',
                yesAction: function () {
                    apiSvc.deleteInvoice(invoice.id)
                        .then(
                            function () {
                                loadInvocies();
                            },
                            function (error) {
                                $log.error(error);
                            });
                }
            });
        }

        function openDetails(invoice) {
            $window.location.href = '/invoices/'+invoice.id;
        }

        function create() {
            $rootScope.$broadcast('createInvoice', {});
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
            loadInvocies();
        }

        function showNoResults() {
            return vm.filter.search && !vm.model.invoices.length;
        }
    }
})();
