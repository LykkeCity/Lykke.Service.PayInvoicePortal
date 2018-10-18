import { Component, OnInit } from '@angular/core';
import { SignupApi } from '../../services/api/SignupApi';
import { SignupModel } from './SignupModel';
import { ROUTE_GET_STARTED_PAGE } from '../../constants/routes';
import { isValidEmail } from '../../utils/utils';
import { ErrorResponse } from '../../models/ErrorResponse';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: SignupComponent.Selector,
  templateUrl: './Signup.html'
})
export class SignupComponent implements OnInit {
  static readonly Selector = 'lp-signup';
  constructor(private api: SignupApi) {}

  model = new SignupModel();
  view = new View();
  validation = new Validation();
  isInvalidEmail(): boolean {
    return !isValidEmail(this.model.employeeEmail);
  }

  ngOnInit(): void {
    this.model.hostUrl = location.origin;
  }

  onToggleVisibilityPassword(): void {
    this.view.isVisiblePassword = !this.view.isVisiblePassword;
  }

  onSubmit(): void {
    this.view.isLoading = true;
    this.validation.reset();

    this.api.signup(this.model).subscribe(
      res => {
        window.location.href = `${ROUTE_GET_STARTED_PAGE}?email=${this.model.employeeEmail}`;
      },
      (httpResponseError: HttpErrorResponse) => {
        this.view.isLoading = false;

        const errorResponse = httpResponseError.error as ErrorResponse;

        if (errorResponse) {
          if (errorResponse.errorMessage) {
            switch (errorResponse.errorMessage) {
              case 'MerchantExist':
                this.validation.merchantExistError = true;
                return;
              case 'MerchantEmailExist':
              case 'EmployeeEmailExist':
                this.validation.emailExistError = true;
                return;
              case 'EmailNotSent':
                this.validation.emailNotSent = true;
                return;
            }
          } else if (
            errorResponse.modelErrors &&
            Object.keys(errorResponse.modelErrors).length
          ) {
            this.validation.hasModelErrors = true;
            this.validation.modelErrors = errorResponse.modelErrors;
            return;
          }
        }

        this.validation.unexpectedError = true;
      }
    );
  }
}

class View {
  agreeCheckbox: boolean;
  isVisiblePassword: boolean;
  isLoading: boolean;

  constructor() {
    this.agreeCheckbox = true;
  }
}

class Validation {
  merchantExistError: boolean;
  emailExistError: boolean;
  emailNotSent: boolean;
  hasModelErrors: boolean;
  modelErrors: {};
  unexpectedError: boolean;
  reset(): void {
    this.merchantExistError = false;
    this.emailExistError = false;
    this.emailNotSent = false;
    this.hasModelErrors = false;
    this.modelErrors = {};
    this.unexpectedError = false;
  }
}
