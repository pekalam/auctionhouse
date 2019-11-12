import { Component, OnInit, ViewChild } from '@angular/core';
import { FormGroup, FormControl, Validators, ValidatorFn, AbstractControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthenticationStateService } from '../../../core/services/AuthenticationStateService';
import { SignInCommand } from '../../../core/commands/SignInCommand';



@Component({
  selector: 'app-sign-in-page',
  templateUrl: './sign-in-page.component.html',
  styleUrls: ['./sign-in-page.component.scss']
})
export class SignInPageComponent implements OnInit {

  @ViewChild('signInForm', {static: true})
  signInForm;

  validPassword = true;
  form = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required])
  });
  private redirectUrl: string;

  constructor(private activatedRoute: ActivatedRoute, private signInCommand: SignInCommand, private router: Router) {
    if (this.router.getCurrentNavigation().extras.state) {
      this.redirectUrl = this.router.getCurrentNavigation().extras.state.redirect;
    }
  }

  ngOnInit() {
  }

  onSignIn() {

    if (this.form.valid) {
      this.signInCommand
        .execute(this.form.value.username, this.form.value.password)
        .subscribe((userIdentity) => {
          if (this.redirectUrl) {
            this.router.navigateByUrl(this.redirectUrl);
          } else {
            this.router.navigate(['home']);
          }
        }, (err) => {
          this.validPassword = false;
          let lastUsername = this.form.value.username;
          this.signInForm.resetForm();
          this.form.reset({username: lastUsername, password: ''});
        });
    }
  }

}
