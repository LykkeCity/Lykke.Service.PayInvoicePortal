import { Component, OnInit, OnDestroy, NgZone } from '@angular/core';
import { PagerModel } from '../../models/PagerModel';
import { ConfirmModalService } from '../../services/ConfirmModalService';
import { PaymentsFilterModel, PaymentsFilterLocalStorageKeys } from './PaymentsFilter/PaymentsFilterModel';
import { PaymentModel } from '../../models/Payment/PaymentModel';
import { PaymentsApi } from '../../services/api/PaymentsApi';
import { interval, Subscription } from 'rxjs';
import { AssetModel } from '../../models/AssetModel';
import { PaymentsResponse } from './PaymentsResponse';

declare const pubsubEvents: any;
declare const moment: any;

@Component({
  selector: PaymentsComponent.Selector,
  templateUrl: './Payments.html'
})
export class PaymentsComponent implements OnInit, OnDestroy, IPaymentsHandlers {
  static readonly Selector = 'lp-payments';

  model = new PaymentsModel();
  filter = new PaymentsFilterModel();
  view = new View();
  pager = new PagerModel(20);

  // reload every 5 minutes
  private readonly reloadInterval = interval(5 * 60 * 1000);
  private reloadSubscriber: Subscription;
  private getPaymentsSubscriber: Subscription;

  showMore(): void {
    this.pager.page++;
    this.loadPayments(LoadPaymentsCaller.ShowMore);
  }

  onFilterChanged(filter: PaymentsFilterModel) {
    this.pager.resetPage();
    this.loadPayments();
  }

  paymentRemoved(index: number): void {
    if (this.model.payments[index]) {
      this.model.payments.splice(index, 1);
    }
  }

  onInvoiceCreated(): void {
    this.zone.run(() => {
      this.loadPayments();
    });
  }

  exportToCsv(): void {
    const url = '/api/export/payments';

    const period = `period=${this.filter.period}`;
    const type = `type=${this.filter.type}`;
    const statuses = `statuses=${this.filter.status}`;
    const searchText = `searchText=${this.filter.searchText || ''}`;
    const params = `?${period}&${type}&${statuses}&${searchText}`;

    window.open(`${url}${params}`);
  }

  constructor(
    private zone: NgZone,
    private api: PaymentsApi,
    private confirmModalService: ConfirmModalService
  ) {}

  ngOnInit(): void {
    this.view.isLoadingBaseAsset = true;

    this.api.getBaseAsset().subscribe(
      res => {
        this.model.baseAsset = res;
        this.view.isLoadingBaseAsset = false;
      },
      error => {
        console.error(error);
        this.view.isLoadingBaseAsset = false;
      }
    );

    // init filter values
    const period = localStorage.getItem(PaymentsFilterLocalStorageKeys.Period);
    const type = localStorage.getItem(PaymentsFilterLocalStorageKeys.Type);
    const status = localStorage.getItem(PaymentsFilterLocalStorageKeys.Status);
    const searchText = localStorage.getItem(PaymentsFilterLocalStorageKeys.SearchText);

    if (period) {
      this.filter.period = Number(period);
    }

    if (type) {
      this.filter.type = Number(type);
    }

    if (status) {
      this.filter.status = status;
    }

    if (searchText) {
      this.filter.searchText = searchText;
    }

    // load data and make subsriptions
    this.loadPayments();

    this.reloadSubscriber = this.reloadInterval.subscribe(_ =>
      this.loadPayments()
    );

    if ((window as any).pubsubEvents) {
      pubsubEvents.on('invoiceCreated', () => this.onInvoiceCreated());
    }
  }

  ngOnDestroy(): void {
    if (this.reloadSubscriber) {
      this.reloadSubscriber.unsubscribe();
    }
    if (this.getPaymentsSubscriber) {
      this.getPaymentsSubscriber.unsubscribe();
    }
    if ((window as any).pubsubEvents) {
      pubsubEvents.off('invoiceCreated');
    }
  }

  private loadPayments(caller = LoadPaymentsCaller.Filter): void {
    switch (caller) {
      case LoadPaymentsCaller.ShowMore:
        this.view.isShowMoreLoading = true;
        break;
      default:
        this.view.isLoading = true;
        break;
    }

    this.view.showNoResults = false;

    const params = {
      period: this.filter.period,
      type: this.filter.type,
      statuses: this.filter.status,
      searchText: this.filter.searchText || '',
      take: this.pager.pageSize * this.pager.page
    };

    if (this.getPaymentsSubscriber) {
      this.getPaymentsSubscriber.unsubscribe();
    }

    this.getPaymentsSubscriber = this.api.getPayments(params).subscribe(
      (res: PaymentsResponse) => {
        this.model.payments = new Array<PaymentModel>();

        for (let i = 0, l = res.payments.length; i < l; i++) {
          const item = res.payments[i];

          // need to initialize via constructor in order only getter properties exist
          const payment = new PaymentModel(
            item.id,
            item.number,
            moment(item.createdDate),
            item.clientName,
            item.clientEmail,
            item.amount,
            item.settlementAsset,
            item.status,
            moment(item.dueDate)
          );

          this.model.payments.push(payment);
        }

        this.view.hasMorePayments = res.hasMorePayments;

        if (this.view.isFirstLoading) {
          this.view.hasPayments = res.hasAnyPayment;
          this.view.isFirstLoading = false;
        }

        if (res.payments.length === 0) {
          this.view.showNoResults = true;
        }

        this.view.isShowMoreLoading = false;
        this.view.isLoading = false;
      },
      error => {
        console.error(error);
        this.confirmModalService.showErrorModal();
        this.view.isShowMoreLoading = false;
        this.view.isLoading = false;
      }
    );
  }
}

class PaymentsModel {
  baseAsset: AssetModel;
  payments: PaymentModel[];
  constructor() {
    this.baseAsset = new AssetModel();
    this.payments = [];
  }
}

interface IPaymentsHandlers {
  showMore: () => void;
  onFilterChanged: (_: any) => void;
  paymentRemoved: (_: number) => void;
  exportToCsv: () => void;
}

enum LoadPaymentsCaller {
  Filter,
  ShowMore
}

class View {
  isFirstLoading: boolean;
  isLoading: boolean;
  isShowMoreLoading: boolean;
  isLoadingBaseAsset: boolean;
  hasPayments: boolean;
  showNoResults: boolean;
  hasMorePayments: boolean;
  get showNoInvoices(): boolean {
    return !this.isFirstLoading && !this.hasPayments;
  }
  constructor() {
    this.isFirstLoading = true;
  }
}