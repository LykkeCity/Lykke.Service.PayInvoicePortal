import { BrowserModule } from '@angular/platform-browser';
import { NgModule, ApplicationRef } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { ResetPasswordComponent } from './components/ResetPassword/ResetPassword';
import { ValidatorPasswordEqualledDirective } from './components/ResetPassword/ValidatorPasswordEqualled.directive';

import { ResetPasswordApi } from './services/api/ResetPasswordApi';

@NgModule({
  declarations: [
    ResetPasswordComponent,
    ValidatorPasswordEqualledDirective
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule
  ],
  providers: [
    ResetPasswordApi
  ],
  entryComponents: [
    ResetPasswordComponent
  ]
})

export class AppModule {
  // app - reference to the running application (ApplicationRef)
  ngDoBootstrap(app: ApplicationRef) {
    // define the possible bootstrap components
    // with their selectors (html host elements)
    const options = {};

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
