import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class CheckoutApi extends BaseApi {
  getDetails(invoiceId: string): Observable<any> {
    return this.get(`api/checkout/${invoiceId}`);
  }

  refreshDetails(invoiceId: string): Observable<any> {
    return this.post(`api/checkout/refresh/${invoiceId}`);
  }

  getStatus(invoiceId: string): Observable<any> {
    return this.get(`api/checkout/${invoiceId}/status`);
  }

  changePaymentAsset(
    invoiceId: string,
    paymentAssetId: string
  ): Observable<any> {
    return this.post(`api/checkout/changeasset/${invoiceId}/${paymentAssetId}`);
  }
}
