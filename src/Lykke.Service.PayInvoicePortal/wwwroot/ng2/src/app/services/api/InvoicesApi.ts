import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class InvoicesApi extends BaseApi {

  getDetails(invoiceId: string): Observable<any> {
    return this.get(`api/invoices/details/${invoiceId}`);
  }

  getInvoice(invoiceId: string): Observable<any> {
    return this.get(`api/invoices/${invoiceId}`);
  }

  deleteInvoice(invoiceId: string): Observable<any> {
    return this.delete(`api/invoices/${invoiceId}`);
  }
}
