import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { SignUpCommand, SignUpCommandArgs } from '../../../core/commands/SignUpCommand';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { CheckUsernameQuery, CheckUsernameQueryResult } from '../../../core/queries/CheckUsernameQuery';
import { Subject, Observable, empty, EMPTY } from 'rxjs';
import { debounceTime, switchMap, catchError, tap } from 'rxjs/operators';
import { PasswordStrength } from 'src/app/core/utils/PasswordStrength';

@Component({
  selector: 'app-sign-up-page',
  templateUrl: './sign-up-page.component.html',
  styleUrls: ['./sign-up-page.component.scss']
})
export class SignUpPageComponent implements OnInit {

  private usernameVal = new Subject<string>();
  private checkUsernameResultObsrv = new Observable<CheckUsernameQueryResult>();
  validPassword = true;
  form = new FormGroup({
    username: new FormControl('', [Validators.required, Validators.minLength(4)]),
    password: new FormControl('', [Validators.required, Validators.minLength(4)]),
    confirmPassword: new FormControl('', [Validators.required, Validators.minLength(4)]),
    email: new FormControl('', [Validators.email]),
  });

  showOkUsername = false;
  showInvalidUsername = false;
  showUsernameAlreadyExists = false;
  showLoading = false;

  passwordMatch = false;
  strongPassword = false;

  constructor(private signUpCommand: SignUpCommand, private checkUsernameQuery: CheckUsernameQuery, private router: Router) {

    this.checkUsernameResultObsrv = this.usernameVal.pipe(
      debounceTime(500),
      switchMap((v) => {
        this.showLoading = true;
        return this.checkUsernameQuery.execute(v).pipe(catchError((err) => {
          this.showLoading = false;
          return EMPTY;
        }));
      })
    );

    this.checkUsernameResultObsrv.subscribe((v) => {
      this.showOkUsername = !v.exist;
      this.showInvalidUsername = v.exist;
      this.showLoading = false;
    });
  }

  ngOnInit() {
  }

  get validUsername(): boolean{
    return this.showOkUsername && !this.showLoading && !this.showInvalidUsername;
  }

  onUsernameChange() {
    if (this.form.controls.username.valid) {
      this.showInvalidUsername = false;
      this.showOkUsername = false;
      this.usernameVal.next(this.form.value.username);
    } else {
      this.showInvalidUsername = false;
      this.showOkUsername = false;
      this.showLoading = false;
    }
  }

  onConfirmType() {
    if (this.form.value.confirmPassword !== this.form.value.password) {
      this.passwordMatch = false;
      this.form.controls.confirmPassword.setErrors(Object.assign(
        this.form.controls.confirmPassword.errors || {}, { match: true }));
    } else {
      this.passwordMatch = true;
      this.form.controls.confirmPassword.setErrors(null);
    }
  }

  private handleSignUpError(err: HttpErrorResponse) {
    this.showUsernameAlreadyExists = err.status === 409;
    if (!this.showUsernameAlreadyExists) {
      this.router.navigate(['/error'], {
        state: {
          msg: err.error
        }
      });
    }
    console.log(err);
  }

  onSignUpClick() {
    if (this.form.valid && this.strongPassword && this.validUsername) {
      const commandArgs = {
        username: this.form.value.username,
        password: this.form.value.password,
        email: this.form.value.email
      };
      this.signUpCommand
        .execute(commandArgs)
        .subscribe((v) => this.router.navigate(['/sign-in']), (err: HttpErrorResponse) => this.handleSignUpError(err));
    }
  }

}
