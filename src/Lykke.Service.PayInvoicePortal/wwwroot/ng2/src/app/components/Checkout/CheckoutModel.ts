import { CheckoutResponse } from './CheckoutResponseModel';
import { AssetItemViewModel } from 'src/app/models/AssetItemViewModels';

const qrUrlBegin = 'https://chart.googleapis.com/chart?chs=';
const qrUrlEnd= '&chld=L|0&cht=qr&chl=';

export class CheckoutModel extends CheckoutResponse {
  message = '';
  qrCodeData = '';
  paymentAssets: AssetItemViewModel[] = [];
  const: Const;
  header: Header;
  pie: Pie;
  timer: Timer;
  statusTimeout: any;
  revertPaymentAssetTrigger = 0;

  constructor() {
    super();
    this.const = new Const();
    this.header = new Header();
    this.pie = new Pie();
    this.timer = new Timer();
  }
}

class Const {
  qrUrlSize220 = `${qrUrlBegin}220x220${qrUrlEnd}`;
  qrUrlSize152 = `${qrUrlBegin}152x152${qrUrlEnd}`;
}

class Timer {
  interval: any;
  total = 0;
  seconds = 0;
  mins = 0;
  isExtended: boolean;
}

class Header {
  title = '';
  message = '';
  color = '';
  icon = '';
}

class Pie {
  transform1 = '';
  transform2 = '';
}
