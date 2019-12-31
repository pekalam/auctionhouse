import { Component, OnInit, ViewChild } from '@angular/core';
import { FormGroup, FormControl, Validators, ValidatorFn, AbstractControl } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthenticationStateService } from '../../../core/services/AuthenticationStateService';
import { SignInCommand } from '../../../core/commands/auth/SignInCommand';



@Component({
  selector: 'app-sign-in-page',
  templateUrl: './sign-in-page.component.html',
  styleUrls: ['./sign-in-page.component.scss']
})
export class SignInPageComponent implements OnInit {

  @ViewChild('signInForm', { static: true })
  signInForm;

  validPassword = true;
  form = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required])
  });
  private redirect: any;

  constructor(private signInCommand: SignInCommand, private router: Router) {
    if (this.router.getCurrentNavigation().extras.state) {
      this.redirect = this.router.getCurrentNavigation().extras.state.redirect;
    }
  }

  ngOnInit() {
  }

  onSignIn() {

    if (this.form.valid) {
      this.signInCommand
        .execute(this.form.value.username, this.form.value.password)
        .subscribe((userIdentity) => {
          console.log(this.redirect);

          if (this.redirect) {
            this.router.navigateByUrl(this.redirect);
          } else {
            this.router.navigate(['home']);
          }
        }, (err: HttpErrorResponse) => {
          if (err.status === 401) {
            this.validPassword = false;
            let lastUsername = this.form.value.username;
            this.signInForm.resetForm();
            this.form.reset({ username: lastUsername, password: '' });
          }
        });
    }
  }

}
