import { AssetItemViewModel } from 'src/app/models/AssetItemViewModels';
import { AssetModel } from 'src/app/models/AssetModel';

export class RefundDialogModel {
  paymentAsset: AssetModel;
  selectedPaymentAssetId: string;
  paymentAssets: AssetItemViewModel[] = [];
  selectedWalletAddress: string;
  sourceWalletAddresses: AssetItemViewModel[] = [];
  manualAddress: string;
  amount: number;
}
