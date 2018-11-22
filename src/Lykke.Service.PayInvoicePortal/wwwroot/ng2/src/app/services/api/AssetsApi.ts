import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class AssetsApi extends BaseApi {
  getSettlementAssets(): Observable<any> {
    return this.get('api/settlementAssets');
  }
}
