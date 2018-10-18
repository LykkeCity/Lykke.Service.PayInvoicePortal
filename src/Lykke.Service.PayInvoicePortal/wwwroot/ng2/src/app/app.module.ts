import { BrowserModule } from '@angular/platform-browser';
import { NgModule, ApplicationRef } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// common
import { AuthInterceptor } from './interceptors/AuthInterceptor';
import { CopyTextDirective } from './directives/CopyText.directive';
import { DebounceDirective } from './directives/Debounce.directive';

import { ChangePasswordComponent } from './components/ChangePassword/ChangePassword';
import { ChangePasswordApi } from './services/api/ChangePasswordApi';
import { ValidatorOldPasswordNotEqualledDirective } from './components/ChangePassword/ValidatorOldPasswordNotEqualled.directive';

import { ResetPasswordComponent } from './components/ResetPassword/ResetPassword';
import { ResetPasswordApi } from './services/api/ResetPasswordApi';
import { ValidatorPasswordEqualledDirective } from './components/ResetPassword/ValidatorPasswordEqualled.directive';

import { SignupComponent } from './components/Signup/Signup';
import { SignupApi } from './services/api/SignupApi';
import { EmailValidatorDirective } from './directives/validators/EmailValidator.directive';

import { SettingsComponent } from './components/Settings/Settings';
import { SettingsApi } from './services/api/SettingsApi';
import { SelectPickerComponent } from './components/SelectPicker/SelectPicker';

import { ConfirmModalComponent } from './components/ConfirmModal/ConfirmModal';
import { ConfirmModalService } from './services/ConfirmModalService';

import { PaymentsComponent } from './components/Payments/Payments';
import { PaymentsApi } from './services/api/PaymentsApi';
import { PaymentsBalanceComponent } from './components/Payments/PaymentsBalance/PaymentsBalance';
import { PaymentsFilterComponent } from './components/Payments/PaymentsFilter/PaymentsFilter';
import { PaymentsStatisticComponent } from './components/Payments/PaymentsStatistic/PaymentsStatistic';
import { PaymentsTableComponent } from './components/Payments/PaymentsTable/PaymentsTable';
import { PaymentStatusCssService } from './services/Payment/PaymentStatusCssService';

import { InvoiceDetailsComponent } from './components/InvoiceDetails/InvoiceDetails';
import { InvoicesApi } from './services/api/InvoicesApi';
import { FileService } from './services/FileService';
import { HistoryItemComponent } from './components/InvoiceDetails/History/HistoryItem';
import { InvoiceDetailsShareDialogComponent } from './components/InvoiceDetails/InvoiceDetailsShareDialog/InvoiceDetailsShareDialog';
import { EmailApi } from './services/api/EmailApi';

@NgModule({
  declarations: [
    InvoiceDetailsShareDialogComponent,
    HistoryItemComponent,
    InvoiceDetailsComponent,
    PaymentsComponent,
    PaymentsBalanceComponent,
    PaymentsFilterComponent,
    PaymentsStatisticComponent,
    PaymentsTableComponent,
    ChangePasswordComponent,
    ConfirmModalComponent,
    SelectPickerComponent,
    SettingsComponent,
    SignupComponent,
    EmailValidatorDirective,
    ResetPasswordComponent,
    CopyTextDirective,
    DebounceDirective,
    ValidatorOldPasswordNotEqualledDirective,
    ValidatorPasswordEqualledDirective
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule
  ],
  providers: [
    FileService,
    PaymentStatusCssService,
    ConfirmModalService,
    EmailApi,
    InvoicesApi,
    PaymentsApi,
    ChangePasswordApi,
    SettingsApi,
    SignupApi,
    ResetPasswordApi,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  entryComponents: [
    InvoiceDetailsComponent,
    PaymentsComponent,
    ChangePasswordComponent,
    ConfirmModalComponent,
    SettingsComponent,
    SignupComponent,
    ResetPasswordComponent
  ]
})
export class AppModule {
  // app - reference to the running application (ApplicationRef)
  ngDoBootstrap(app: ApplicationRef) {
    // define the possible bootstrap components
    // with their selectors (html host elements)
    const options = {};

    // modal component should be first as it is global component
    options[ConfirmModalComponent.Selector] = ConfirmModalComponent;
    // other components
    options[InvoiceDetailsComponent.Selector] = InvoiceDetailsComponent;
    options[PaymentsComponent.Selector] = PaymentsComponent;
    options[ChangePasswordComponent.Selector] = ChangePasswordComponent;
    options[SettingsComponent.Selector] = SettingsComponent;
    options[SignupComponent.Selector] = SignupComponent;
    options[ResetPasswordComponent.Selector] = ResetPasswordComponent;

    // tslint:disable-next-line:no-shadowed-variable
    for (const key in options) {
      if (
        options.hasOwnProperty(key) &&
        document.getElementsByTagName(key).length
      ) {
        const component = options[key];
        // bootstrap the application with the selected component
        app.bootstrap(component);
      }
    }
  }
}
