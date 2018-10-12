import { FileModel } from '../../models/FileModel';
import { HistoryItemModel } from './History/HistoryItemModel';

export class InvoiceModel {
  constructor(
    public id: string = '',
    public number: string = '',
    public clientName: string = '',
    public clientEmail: string = '',
    public amount: number = 0,
    public dueDate: Date = null,
    public status: string = '',
    public settlementAsset: string = '',
    public settlementAssetDisplay: string = '',
    public settlementAssetAccuracy: string= '',
    public paymentAsset: string = '',
    public paymentAssetNetwork: string = '',
    public walletAddress: string = '',
    public createdDate: string = '',
    public note: string = ''
  ) {
    this.files = new Array<FileModel>();
    this.history = new Array<HistoryItemModel>();
  }

  public files: FileModel[];
  public history: HistoryItemModel[];
}
