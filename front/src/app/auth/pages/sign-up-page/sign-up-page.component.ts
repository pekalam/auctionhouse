import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { SignUpCommand, SignUpCommandArgs } from '../../../core/commands/SignUpCommand';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { CheckUsernameQuery, CheckUsernameQueryResult } from '../../../core/queries/CheckUsernameQuery';
import { Subject, Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';

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
    password: new FormControl('', [Validators.required, Validators.minLength(4)])
  });

  showOkUsername = false;
  showInvalidUsername = false;
  showLoading = false;

  constructor(private signUpCommand: SignUpCommand, private checkUsernameQuery: CheckUsernameQuery, private router: Router) {

    this.checkUsernameResultObsrv = this.usernameVal.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap((v) => {
        console.log("chck");

          return this.checkUsernameQuery.execute(v);
      })
    );

    this.checkUsernameResultObsrv.subscribe((v) => {
      this.showOkUsername = !v.exist;
      this.showInvalidUsername = v.exist;
      this.showLoading = false;
    }, (err: HttpErrorResponse) => { });
  }

  ngOnInit() {
  }

  onUsernameChange() {
    if (this.form.controls.username.valid) {
      this.showInvalidUsername = false;
      this.showOkUsername = false;
      this.showLoading = true;
      this.usernameVal.next(this.form.value.username);
    }
  }

  onSignUpClick() {
    if (this.form.valid) {
      let commandArgs = new SignUpCommandArgs(
        this.form.value.username,
        this.form.value.password,
        '1234'
      );
      this.signUpCommand.execute(commandArgs)
        .subscribe((v) => {
          this.router.navigate(['/sign-in']);
        }, (err: HttpErrorResponse) => {
          console.log('sign up error');
          console.log(err);

        });

    }
  }

}
