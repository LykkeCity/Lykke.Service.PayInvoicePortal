import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class UserApi extends BaseApi {

  getUserInfo(): Observable<any> {
    return this.get('api/user');
  }
}
