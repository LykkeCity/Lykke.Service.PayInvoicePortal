import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SignupModel } from '../../components/Signup/SignupModel';

@Injectable()
export class SignupApi extends BaseApi {
  signup(model: SignupModel): Observable<any> {
    return this.post('api/signup', model);
  }
}
