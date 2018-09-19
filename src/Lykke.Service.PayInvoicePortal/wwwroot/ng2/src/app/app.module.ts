import { BrowserModule } from '@angular/platform-browser';
import { NgModule, ApplicationRef } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AuthInterceptor } from './interceptors/AuthInterceptor';

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
import { CopyTextDirective } from './directives/CopyText.directive';

@NgModule({
  declarations: [
    ConfirmModalComponent,
    SelectPickerComponent,
    SettingsComponent,
    SignupComponent,
    EmailValidatorDirective,
    ResetPasswordComponent,
    CopyTextDirective,
    ValidatorPasswordEqualledDirective
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule
  ],
  providers: [
    ConfirmModalService,
    SettingsApi,
    SignupApi,
    ResetPasswordApi,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
  ],
  entryComponents: [
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

    options[ConfirmModalComponent.Selector] = ConfirmModalComponent;
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
