import { AssetItemViewModel } from 'src/app/models/AssetItemViewModels';
import { FileModel } from 'src/app/models/FileModel';

declare const moment: any;

export class InvoiceEditModel {
  private defaultDueDate = moment().add(2, 'days');
  isNewInvoice = true;
  id = '';
  number = '';
  status = '';
  client = '';
  email = '';
  selectedSettlementAssetId = '';
  settlementAssets: Array<AssetItemViewModel> = [];
  amount = 0;
  dueDate: Date = this.defaultDueDate;
  note = '';
  files: File[] | FileModel[] = [];

  reset(): void {
    this.isNewInvoice = true;
    this.id = '';
    this.number = '';
    this.status = '';
    this.client = '';
    this.email = '';
    this.amount = null;
    this.dueDate = this.defaultDueDate;
    this.note = '';
    this.files = [];
  }

  copyFrom(model: InvoiceEditModel): void {
    this.id = model.id;
    this.status = model.status;
    this.number = model.number;
    this.client = model.client;
    this.email = model.email;
    this.selectedSettlementAssetId = model.selectedSettlementAssetId;
    this.amount = model.amount;
    this.dueDate = moment().diff(model.dueDate, 'days') > 0 ? moment() : model.dueDate;
    this.note = model.note;
    this.files = model.files;
  }
}
