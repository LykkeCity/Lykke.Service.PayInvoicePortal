(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoicesCtrl', invoicesCtrl);

    invoicesCtrl.$inject = ['$scope', '$window', '$rootScope', '$interval', '$log', 'apiSvc', 'statusSvc', 'confirmModalSvc'];

    function invoicesCtrl($scope, $window, $rootScope, $interval, $log, apiSvc, statusSvc, confirmModalSvc) {
        var vm = this;

        vm.model = {
            isFirstLoading: true,
            isSupervising: ($window.location.pathname.indexOf("Supervising") != -1),
            invoices: []
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
            exportToCsv: exportToCsv,
            getStatusCss: statusSvc.getStatusCss,
            showMore: showMore,
            canShowMore: canShowMore
        };

        vm.view = {
            isLoading: false,
            isSearching: false,
            get showNoResults() {
                return showNoResults();
            },
            hasInvoices: true
        };

        activate();

        function activate() {
            if (vm.model.isSupervising)
                loadSupervisingInvocies();

            $scope.$watch(
                function () { return vm.filter.search; },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        vm.view.isSearching = true;
                        resetPage();
                        vm.model.isSupervising ?
                            loadSupervisingInvocies().finally(function () {
                                vm.view.isSearching = false;
                            }) :
                            loadInvocies().finally(function () {
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
                        vm.model.isSupervising ? loadSupervisingInvocies() : loadInvocies();
                    }
                }
            );
            $scope.$watch(
                function () { return vm.filter.status; },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        vm.model.isSupervising ? loadSupervisingInvocies() : loadInvocies();
                    }
                }
            );

            if (vm.model.isSupervising)
                $interval(loadSupervisingInvocies, 5 * 60 * 1000);
        }

        function loadSupervisingInvocies() {
            vm.view.isLoading = true;

            return apiSvc.getSupervisingInvoices(vm.filter.search,
                    vm.filter.period,
                    getFilterStatuses(),
                    vm.filter.sortField,
                    vm.filter.sortAscending,
                    0,
                    vm.pager.pageSize * vm.pager.page)
                .then(
                    function (data) {
                        updateData(data);
                    },
                    function (error) {
                        $log.error(error);
                    })
                .finally(function () {
                    vm.view.isLoading = false;
                });
        }

        function updateData(data) {
            updateList(data.list);

            if (vm.model.isFirstLoading){
                vm.view.hasInvoices = data.list.items.length ? true : false;
                vm.model.isFirstLoading = false;
            }
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
                vm.model.isSupervising
            );
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
            if (vm.model.isSupervising)
                loadSupervisingInvocies();
        }

        function showNoResults() {
            return vm.filter.search && !vm.model.invoices.length;
        }
    }
})();
