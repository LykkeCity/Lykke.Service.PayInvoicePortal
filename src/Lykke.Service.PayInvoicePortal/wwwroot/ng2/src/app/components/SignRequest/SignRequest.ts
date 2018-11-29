import { Component, OnInit, Renderer2, ViewChild } from '@angular/core';
import { ConfirmModalService } from 'src/app/services/ConfirmModalService';
import { SignRequestInitialInfoModel } from './SignRequestInitialInfoModel';
import { SignRequestModel } from './SignRequestModel';
import { SignRequestApi, SignRequestResultModel } from 'src/app/services/api/SignRequestApi';
import { FormGroup } from '@angular/forms';

@Component({
  selector: SignRequestComponent.Selector,
  templateUrl: './SignRequest.html'
})
export class SignRequestComponent implements OnInit, ISignRequestComponentHandlers {
  static readonly Selector = 'lp-sign-request';

  @ViewChild('signRequestForm')
  signRequestForm: FormGroup;

  model = new SignRequestModel();
  view = new View();

  constructor(
    private api: SignRequestApi
  ) {}

  ngOnInit(): void {
    if ((window as any).signRequestInitialInfo) {
      const initInfo = (window as any).signRequestInitialInfo as SignRequestInitialInfoModel;
      this.model.lykkeMerchantId = initInfo.merchantId;
      this.model.apiKey = initInfo.apiKey;
    }
  }

  open(): void {
    this.view.show = true;
  }

  close(): void {
    this.view.show = false;
  }

  overlayClick(e: MouseEvent): void {
    if (this.view.show) {
      e.preventDefault();
      e.stopPropagation();

      this.close();
    }
  }

  submit(): void {
    if (this.signRequestForm.invalid) {
      return;
    }

    this.view.isLoading = true;
    this.model.result = '';
    this.model.error = '';

    const model = {
      lykkeMerchantId: this.model.lykkeMerchantId,
      apiKey: this.model.apiKey,
      rsaPrivateKey: this.model.rsaPrivateKey,
      body: this.model.body
    };

    this.api.signRequest(model).subscribe(
      (res: SignRequestResultModel) => {
        this.model.result = res.signedBody;
        this.view.isLoading = false;
      },
      error => {
        console.error(error);
        this.model.error = 'Unable to sign request with provided data. Please review input data or contact support.';
        this.view.isLoading = false;
      }
    );
  }
}

interface ISignRequestComponentHandlers {
  open: () => void;
  close: () => void;
  overlayClick: (_: MouseEvent) => void;
  submit: () => void;
}

class View {
  isLoading: boolean;
  show: boolean;
}
