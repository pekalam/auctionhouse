import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SignInPageComponent } from './pages/sign-in-page/sign-in-page.component';
import { SignUpPageComponent } from './pages/sign-up-page/sign-up-page.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '../material.module';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { AuthRoutingModule } from './auth-routing.module';
import { CoreModule } from '../core/core.module';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/reset-password/reset-password.component';
import { CheckResetCodePageComponent } from './pages/check-reset-code-page/check-reset-code-page.component';
import { PasswordMeterComponent } from './components/password-meter/password-meter.component';



@NgModule({
  declarations: [SignInPageComponent, SignUpPageComponent, ForgotPasswordComponent, ResetPasswordComponent, CheckResetCodePageComponent, PasswordMeterComponent],
  imports: [
    CommonModule,
    CoreModule,
    ReactiveFormsModule,
    RouterModule,
    MaterialModule,
    SharedModule,
    AuthRoutingModule
  ],
  exports: [SignUpPageComponent, SignInPageComponent]
})
export class AuthModule { }
