export class SendInvoiceEmailRequest {
  constructor(
    public invoiceId: string,
    public checkoutUrl: string,
    public emails: string[]
  ) {}
}
