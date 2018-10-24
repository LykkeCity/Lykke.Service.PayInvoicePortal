import { Injectable } from '@angular/core';
import { UserInfoModel } from '../models/UserInfoModel';

@Injectable()
export class UserService {
  user: UserInfoModel;

  constructor() {
    this.user = new UserInfoModel();
  }
}
