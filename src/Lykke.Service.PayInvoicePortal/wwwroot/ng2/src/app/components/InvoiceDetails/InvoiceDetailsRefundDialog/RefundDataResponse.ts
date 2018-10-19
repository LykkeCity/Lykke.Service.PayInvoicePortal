import { AssetModel } from 'src/app/models/AssetModel';

export class RefundDataResponse {
  paymentAsset: AssetModel;
  sourceWalletAddresses: string[] = [];
  amount: number;

  static isValid(res: RefundDataResponse): boolean {
    return res.paymentAsset && res.sourceWalletAddresses.length && res.amount > 0;
  }
}
