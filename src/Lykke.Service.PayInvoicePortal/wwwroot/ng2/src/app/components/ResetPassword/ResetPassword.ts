import { Component, OnInit } from '@angular/core';
import { ResetPasswordModel } from './ResetPasswordModel';
import { nameof, getParameterByName } from '../../utils/utils';
import { ResetPasswordApi } from '../../services/api/ResetPasswordApi';
import { ROUTE_SIGNIN_PAGE } from '../../constants/routes';

@Component({
  selector: ResetPasswordComponent.Selector,
  templateUrl: 'ResetPassword.html'
})

export class ResetPasswordComponent implements OnInit {
  static readonly Selector = 'lp-reset-password';

  constructor(private api: ResetPasswordApi) {}

  private token = '';

  model = new ResetPasswordModel();
  view = new View();
  validation = new Validation();

  ngOnInit(): void {
    this.token = getParameterByName(nameof(() => this.token), window.location.href);
  }

  onToggleVisibilityPassword(): void {
    this.view.isVisiblePassword = !this.view.isVisiblePassword;
  }

  onToggleVisibilityReenterPassword(): void {
    this.view.isVisibleReenterPassword = !this.view.isVisibleReenterPassword;
  }

  onSubmit(): void {
    this.view.isLoading = true;

    const model = {
      token: this.token,
      password: this.model.password
    };

    this.api.resetPassword(model).subscribe(
      res => {
        window.location.href = ROUTE_SIGNIN_PAGE;
      },
      error => {
        console.error(error);
        this.view.isLoading = false;
        this.validation.hasSubmitError = true;
      }
    );
  }
}

class View {
  isVisiblePassword: boolean;
  isVisibleReenterPassword: boolean;
  isLoading: boolean;
}

class Validation {
  invalidReenterPassword: boolean;
  hasSubmitError: boolean;
}
