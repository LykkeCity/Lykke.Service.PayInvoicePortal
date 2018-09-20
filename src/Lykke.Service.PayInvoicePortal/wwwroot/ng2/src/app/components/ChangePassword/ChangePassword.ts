import { Component } from '@angular/core';
import { ROUTE_SIGNIN_PAGE } from '../../constants/routes';
import { ChangePasswordApi } from '../../services/api/ChangePasswordApi';
import { ChangePasswordModel } from './ChangePasswordModel';
import { HttpErrorResponse } from '@angular/common/http/src/response';
import { ErrorResponse } from '../../models/ErrorResponse';

@Component({
  selector: ChangePasswordComponent.Selector,
  templateUrl: './ChangePassword.html'
})

export class ChangePasswordComponent {
  static readonly Selector = 'lp-change-password';

  constructor(private api: ChangePasswordApi) {}

  model = new ChangePasswordModel();
  view = new View();
  validation = new Validation();

  onSubmit(): void {
    this.view.isLoading = true;
    this.validation.reset();

    const model = {
      currentPassword: this.model.currentPassword,
      newPassword: this.model.password
    };

    this.api.changePassword(model).subscribe(
      res => {
        window.location.href = ROUTE_SIGNIN_PAGE;
      },
      (httpResponseError: HttpErrorResponse) => {
        this.view.isLoading = false;

        const errorResponse = httpResponseError.error as ErrorResponse;

        if (errorResponse) {
          if (errorResponse.errorMessage) {
            switch (errorResponse.errorMessage) {
              case 'InvalidCurrentPassword':
                this.validation.invalidCurrentPassword = true;
                return;
            }
          }
        }

        this.validation.unexpectedError = true;
      }
    );
  }
}

class View {
  isVisibleCurrentPassword: boolean;
  isVisiblePassword: boolean;
  isVisibleReenterPassword: boolean;
  isLoading: boolean;
  onToggleVisibilityCurrentPassword(): void {
    this.isVisibleCurrentPassword = !this.isVisibleCurrentPassword;
  }

  onToggleVisibilityPassword(): void {
    this.isVisiblePassword = !this.isVisiblePassword;
  }

  onToggleVisibilityReenterPassword(): void {
    this.isVisibleReenterPassword = !this.isVisibleReenterPassword;
  }
}

class Validation {
  invalidCurrentPassword: boolean;
  unexpectedError: boolean;
  reset(): void {
    this.invalidCurrentPassword = false;
    this.unexpectedError = false;
  }
}
