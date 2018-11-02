import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@aspnet/signalr';
import { BaseApi } from '../api/BaseApi';
import { InvoiceUpdateModel } from 'src/app/models/realtime/InvoiceUpdateModel';

@Injectable()
export class InvoiceUpdateHubService {
  private readonly connection: signalR.HubConnection;
  private readonly subject = new Subject<InvoiceUpdateModel>();

  constructor(
    private baseApi: BaseApi

  ) {
    const baseUrl = this.baseApi.getBaseUrl();

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}ws/invoiceUpdateHub`)
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on(
      'invoiceUpdated',
      (data: InvoiceUpdateModel) => {
        this.subject.next(data);
      }
    );
  }

  initConnection(): void {
    this.connection.start().catch(err => console.error(err));
  }

  getObservable() {
    return this.subject.asObservable();
  }
}
