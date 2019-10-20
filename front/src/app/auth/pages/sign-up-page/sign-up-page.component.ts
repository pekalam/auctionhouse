import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { SignUpCommand, SignUpCommandArgs } from '../../../core/commands/SignUpCommand';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sign-up-page',
  templateUrl: './sign-up-page.component.html',
  styleUrls: ['./sign-up-page.component.scss']
})
export class SignUpPageComponent implements OnInit {

  validPassword = true;
  form = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required])
  });


  constructor(private signUpCommand: SignUpCommand, private router: Router) { }

  ngOnInit() {
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
        }, (err) => {
          console.log('sign up error');
          console.log(err);

        });

    }
  }

}
