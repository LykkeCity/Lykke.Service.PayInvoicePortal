import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class PaymentsApi extends BaseApi {
  getPayments(params): Observable<any> {
    return this.get('api/payments', params);
  }

  getByInvoiceId(invoiceId: string, params): Observable<any> {
    return this.get(`api/payments/byInvoiceId/${invoiceId}`, params);
  }

  getBaseAsset(): Observable<any> {
    return this.get('api/assets/baseAsset');
  }

  deleteInvoice(id: string): Observable<any> {
    return this.delete(`api/invoices/${id}`);
  }
}
