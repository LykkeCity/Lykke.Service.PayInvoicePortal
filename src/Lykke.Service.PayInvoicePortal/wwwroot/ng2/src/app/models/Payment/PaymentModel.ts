import { PaymentType } from './PaymentType';
import { AssetModel } from '../AssetModel';

export class PaymentModel {
  constructor(
    public id: string,
    public number: string,
    public createdDate: Date,
    public clientName: string,
    public clientEmail: string,
    public amount: number,
    public settlementAsset: AssetModel,
    public paidAmount: number,
    public paymentAssetId: string,
    public status: string,
    public dueDate: Date
  ) {}
  isLoadingDeletePayment: boolean;

  get isInvoice(): boolean {
    return this.type === PaymentType.Invoice;
  }
  get type(): PaymentType {
    return this.number ? PaymentType.Invoice : PaymentType.Api;
  }
}
