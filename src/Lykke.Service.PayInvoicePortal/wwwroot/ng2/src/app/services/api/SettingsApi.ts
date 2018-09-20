import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class SettingsApi extends BaseApi {
  getSettings(): Observable<any> {
    return this.get('api/settings');
  }

  setBaseAsset(model): Observable<any> {
    return this.post('api/settings/baseAsset', model);
  }

  generateRsaKeys(): Observable<any> {
    return this.post('api/settings/generateRsaKeys');
  }

  deleteAccount(): Observable<any> {
    return this.delete('api/settings/delete');
  }
}
