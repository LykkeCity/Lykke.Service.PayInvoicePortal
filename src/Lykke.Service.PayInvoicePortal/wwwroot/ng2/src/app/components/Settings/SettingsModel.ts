import { AssetItemViewModel } from '../../models/AssetItemViewModels';

export class SettingsModel {
  merchantDisplayName: string;
  employeeFullName: string;
  employeeEmail: string;
  availableBaseAssets: AssetItemViewModel[] = [];
  baseAssetId: string;
  merchantId: string;
  merchantApiKey: string;
  hasPublicKey: boolean;
  rsaPrivateKey: string;
}
