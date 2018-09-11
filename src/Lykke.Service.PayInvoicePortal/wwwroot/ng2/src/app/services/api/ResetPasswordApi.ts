import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class ResetPasswordApi extends BaseApi {
  resetPassword(model: any): Observable<any> {
    return this.post('api/resetPassword', model);
  }
}
