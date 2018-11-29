import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class SignRequestApi extends BaseApi {

  signRequest(model): Observable<SignRequestResultModel> {
    return this.post('api/signRequest', model);
  }
}

export class SignRequestResultModel {
  signedBody = '';
}
