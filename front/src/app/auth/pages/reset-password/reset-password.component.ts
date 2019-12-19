import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { ResetPasswordCommand } from '../../../core/commands/ResetPasswordCommand';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit {

  private resetCode;
  private email;

  passwordMatch = true;
  strongPassword = false;

  form = new FormGroup({
    password: new FormControl('', [Validators.required, Validators.minLength(4)]),
    confirmPassword: new FormControl('', [Validators.required, Validators.minLength(4)]),
  });

  constructor(private resetPasswordCommand: ResetPasswordCommand, private router: Router) {
    if (this.router.getCurrentNavigation().extras.state) {
      this.resetCode = this.router.getCurrentNavigation().extras.state.resetCode;
      this.email = this.router.getCurrentNavigation().extras.state.email;
    }
  }

  ngOnInit() {
  }

  onConfirmType() {
    if (this.form.value.confirmPassword !== this.form.value.password) {
      this.form.controls.confirmPassword.setErrors(Object.assign(
        this.form.controls.confirmPassword.errors || {}, { match: true }));
    } else {
      this.form.controls.confirmPassword.setErrors(null);
    }
  }


  onFormSubmit() {
    if (this.form.valid) {
      this.resetPasswordCommand.execute(this.resetCode, this.form.value.password, this.email)
        .subscribe((msg) => {
          if (msg.status === 'COMPLETED') {
            this.router.navigateByUrl('/sign-in');
          } else {
            this.router.navigateByUrl('/error');
          }
        });
    }
  }

}
