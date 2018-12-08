import { FileModel } from 'src/app/models/FileModel';

declare const moment: any;

export class CheckoutResponse {
  id = '';
  number = '';
  status = '';
  merchantId = '';
  merchant = '';
  paymentAmount = 0.0;
  settlementAmount = 0.0;
  paymentAssetId = '';
  paymentAssetNetwork = '';
  paymentAssetDisplay = '';
  paymentAssetSelect = '';
  settlementAssetId = '';
  settlementAssetDisplay = '';
  paymentAssetAccuracy = 0;
  settlementAssetAccuracy = 0;
  exchangeRate = 0.0;
  deltaSpread: boolean;
  percents = 0.0;
  pips = 0;
  fee = 0.0;
  dueDate: Date;
  note = '';
  walletAddress = '';
  paymentRequestId = '';
  totalSeconds = 0;
  remainingSeconds = 0;
  extendedTotalSeconds = 0;
  extendedRemainingSeconds = 0;
  paidDate: Date;
  paidAmount = 0.0;
  files: FileModel[] = [];

  constructor() {
    this.dueDate = moment();
    this.paidDate = null;
  }
}
