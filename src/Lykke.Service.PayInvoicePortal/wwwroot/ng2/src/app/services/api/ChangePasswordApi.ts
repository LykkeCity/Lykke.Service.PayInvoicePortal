import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class ChangePasswordApi extends BaseApi {
  changePassword(model: any): Observable<any> {
    return this.post('api/changePassword', model);
  }
}
