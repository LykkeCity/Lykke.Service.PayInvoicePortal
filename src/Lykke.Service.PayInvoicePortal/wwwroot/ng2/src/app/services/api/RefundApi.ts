import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class RefundApi extends BaseApi {

  getRefundData(paymentRequestId: string): Observable<any> {
    return this.get(`api/refund/${paymentRequestId}`);
  }

  refund(model): Observable<any> {
    return this.post('api/refund', model);
  }
}
