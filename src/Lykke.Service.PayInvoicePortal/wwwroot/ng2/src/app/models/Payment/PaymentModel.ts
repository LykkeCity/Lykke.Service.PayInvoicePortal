import { PaymentType } from './PaymentType';
import { AssetModel } from '../AssetModel';

declare const moment: any;

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
  isUpdated: boolean;

  get isInvoice(): boolean {
    return this.type === PaymentType.Invoice;
  }
  get type(): PaymentType {
    return this.number ? PaymentType.Invoice : PaymentType.Api;
  }

  // need to initialize via constructor in order the getter properties exist and dates become moment
  static create(model: PaymentModel) {
    return new PaymentModel(
      model.id,
      model.number,
      moment(model.createdDate),
      model.clientName,
      model.clientEmail,
      model.amount,
      model.settlementAsset,
      model.paidAmount,
      model.paymentAssetId,
      model.status,
      moment(model.dueDate)
    );
  }

  static copyProps(copyTo: PaymentModel, copyFrom: PaymentModel) {
    copyTo.id = copyFrom.id;
    copyTo.number = copyFrom.number;
    copyTo.createdDate = copyFrom.createdDate;
    copyTo.clientName = copyFrom.clientName;
    copyTo.clientEmail = copyFrom.clientEmail;
    copyTo.amount = copyFrom.amount;
    copyTo.settlementAsset = copyFrom.settlementAsset;
    copyTo.paidAmount = copyFrom.paidAmount;
    copyTo.paymentAssetId = copyFrom.paymentAssetId;
    copyTo.status = copyFrom.status;
    copyTo.dueDate = copyFrom.dueDate;
  }

  animate(): void {
    this.isUpdated = true;
    setTimeout(() => this.isUpdated = false, 2000);
  }
}
