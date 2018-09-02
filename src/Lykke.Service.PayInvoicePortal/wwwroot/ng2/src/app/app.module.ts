import { BrowserModule } from '@angular/platform-browser';
import { NgModule, Injector } from '@angular/core';

import { AppComponent } from './app.component';
import { ResetPasswordComponent } from './ResetPassword/ResetPassword';

@NgModule({
  declarations: [
    AppComponent,
    ResetPasswordComponent
  ],
  imports: [
    BrowserModule
  ],
  providers: [],
  entryComponents: [ ResetPasswordComponent ]
})
export class AppModule {
  constructor(private injector: Injector) {}
  ngDoBootstrap() {
  }
}
