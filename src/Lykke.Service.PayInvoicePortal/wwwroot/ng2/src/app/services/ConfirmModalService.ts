import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { IConfirmModalDataModel } from '../models/IConfirmModalDataModel';

@Injectable()
export class ConfirmModalService {
  private subject = new Subject<IConfirmModalDataModel>();

  constants = {
    errorTitle: 'Error occured'
  };

  constructor() {}

  openModal(data: IConfirmModalDataModel): void {
    this.subject.next(data);
  }

  showErrorModal(): void {
    this.subject.next({
      title: this.constants.errorTitle,
      content: 'Error occured during executing the action, please contact support.'
    });
  }

  getObservable() {
    return this.subject.asObservable();
  }
}
