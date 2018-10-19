import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class EmailApi extends BaseApi {

  sendInvoiceEmail(model): Observable<any> {
    return this.post('api/email', model);
  }
}
