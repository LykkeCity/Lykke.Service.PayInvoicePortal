import { InvoiceModel } from './InvoiceModel';

export class InvoiceDetailsResponse {
  invoice: InvoiceModel;
  blockchainExplorerUrl: string;
  ethereumBlockchainExplorerUrl: string;
}
