import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class InvoicesApi extends BaseApi {
  getDetails(invoiceId: string): Observable<any> {
    return this.get(`api/invoices/details/${invoiceId}`);
  }

  getInvoice(invoiceId: string): Observable<any> {
    return this.get(`api/invoices/${invoiceId}`);
  }

  add(model: CreateInvoiceRequest, files: File[]) {
    const formData = new FormData();

    for (const key in model) {
      if (model.hasOwnProperty(key)) {
        const prop = model[key];
        formData.append(key, prop);
      }
    }

    if (files && files.length) {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        formData.append('files', file, file.name);
      }
    }

    return this.post('api/invoices', formData);
  }

  update(model: UpdateInvoiceRequest): Observable<any> {
    return this.put('api/invoices', model);
  }

  deleteInvoice(invoiceId: string): Observable<any> {
    return this.delete(`api/invoices/${invoiceId}`);
  }
}

export class CreateInvoiceRequest {
  constructor(
    public isDraft: boolean,
    public number: string,
    public client: string,
    public email: string,
    public amount: number,
    public settlementAssetId: string,
    public dueDate: string,
    public note: string
  ) {}
}

export class UpdateInvoiceRequest extends CreateInvoiceRequest {
  constructor(
    public id: string,
    public isDraft: boolean,
    public number: string,
    public client: string,
    public email: string,
    public amount: number,
    public settlementAssetId: string,
    public dueDate: string,
    public note: string
  ) {
    super(
      isDraft,
      number,
      client,
      email,
      amount,
      settlementAssetId,
      dueDate,
      note
    );
  }
}
