export class RefundRequest {
  constructor(
    public paymentRequestId: string,
    public destinationAddress: string
  ) {}
}
