(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoicesCtrl', invoicesCtrl);

    invoicesCtrl.$inject = ['$scope', '$window', '$rootScope', '$interval', 'apiSvc', 'statusSvc'];

    function invoicesCtrl($scope, $window, $rootScope, $interval, apiSvc, statusSvc) {
        var vm = this;

        vm.intervals = {
            balance: null
        };

        vm.model = {
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
                { id: 'removed', title: 'Removed', values: ['Removed'], count: 0 },
                {
                    id: 'inProgress',
                    title: 'In Progress',
                    values: ['InProgress', 'RefundInProgress', 'SettlementInProgress'],
                    count: 0
                },
                { id: 'paid', title: 'Paid', value: ['Paid'], count: 0 },
                { id: 'underpaid', title: 'Underpaid', values: ['Underpaid'], count: 0 },
                { id: 'overpaid', title: 'Overpaid', values: ['Overpaid'], count: 0 },
                { id: 'latePaid', title: 'LatePaid', values: ['LatePaid'], count: 0 },
                { id: 'refunded', title: 'Refunded', values: ['Refunded'], count: 0 },
                { id: 'settled', title: 'Settled', values: ['Settled'], count: 0 },
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
            pageSize: 10,
            page: 1
        };

        vm.handlers = {
            exportToCsv: exportToCsv,
            getStatusRowCss: statusSvc.getStatusRowCss,
            canRemove: canRemove,
            remove: remove,
            showMore: showMore,
            canShowMore: canShowMore,
            openDetails: openDetails,
            sort: sort,
            create: create
        };

        activate();

        function activate() {
            $scope.$watch(
                function () { return vm.filter.search; },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        resetPage();
                        loadInvocies();
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

            $scope.$on('invoiceGenerated', function (evt, data) {
                loadInvocies();
            });

            $scope.$on('invoiceDraftCreated', function (evt, data) {
                loadInvocies();
            });

            $interval(loadInvocies, 5 * 60 * 1000);

            loadInvocies();
        }

        function loadInvocies() {
            apiSvc.getInvoices(vm.filter.search,
                    vm.filter.period,
                    getFilterStatuses(),
                    vm.filter.sortField,
                    vm.filter.sortAscending,
                    0,
                    vm.pager.pageSize * vm.pager.page)
                .then(
                    function(data) {
                        vm.filter.total = data.total;

                        angular.forEach(data.items, function (item, key) {
                            item.dueDate = $window.moment(item.dueDate);
                            item.createdDate = $window.moment(item.createdDate);
                        });
                        
                        vm.model.invoices = data.items;

                        angular.forEach(vm.filter.statuses, function(status, key) {
                            status.count = 0;
                        });

                        angular.forEach(vm.filter.statuses, function(status, key) {
                            angular.forEach(status.values, function(value, key) {
                                var count = data.countPerStatus[value];
                                if (count)
                                    status.count += count;
                            });
                        });

                        if (vm.filter.status) {
                            var selectedStatus = vm.filter.statuses.filter(function(status) {
                                return status.id === vm.filter.status;
                            })[0];

                            if (selectedStatus.count === 0)
                                vm.filter.status = null;
                        }
                    },
                    function(error) {
                        alert(error);
                    });
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

            $.confirm({
                title: 'Are you sure?',
                content: 'Do you really want to delete this invoice?',
                icon: 'fa fa-question-circle',
                animation: 'scale',
                closeAnimation: 'scale',
                opacity: 0.5,
                buttons: {
                    'confirm': {
                        text: 'Yes',
                        btnClass: 'btn-blue',
                        action: function () {
                            apiSvc.removeInvoice(invoice.id)
                                .then(
                                    function() {
                                        loadInvocies();
                                    },
                                    function(error) {
                                        alert(error);
                                    });
                        }
                    },
                    cancel: function () {
                    }
                }
            });
        }

        function openDetails(invoice) {
            $window.location.href = '/home/invoicedetail/?InvoiceId='+invoice.id;
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
    }
})();