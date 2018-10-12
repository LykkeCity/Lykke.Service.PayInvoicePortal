export class HistoryItemModel {
  public author: string;
  public status: string;
  public paymentAmount: number;
  public settlementAmount: number;
  public paidAmount: number;
  public paymentAsset: string;
  public settlementAsset: string;
  public paymentAssetAccuracy: number;
  public settlementAssetAccuracy: number;
  public exchangeRate: number;
  public sourceWalletAddresses: string[];
  public refundWalletAddress: string;
  public refundAmount: number;
  public dueDate: Date;
  public paidDate?: Date;
  public date: Date;
}
