import { Component, OnInit } from '@angular/core';
import { Validators, FormGroup, FormControl } from '@angular/forms';
import { RequestResetPasswordCommand } from '../../../core/commands/auth/RequestResetPasswordCommand';
import { Router } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent implements OnInit {

  form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
  });

  constructor(private requesetResetPasswordCommand: RequestResetPasswordCommand, private router: Router) { }

  ngOnInit() {
  }

  onFormSubmit() {
    if (this.form.valid) {
      const email = this.form.value.email;
      this.requesetResetPasswordCommand.execute(email).subscribe((msg) => {
        if (msg.status === 'COMPLETED') {
          this.router.navigate(['/check-reset-code'], {state: {email}});
        } else {

        }
      }, (err) => this.router.navigateByUrl('/error'));
    }
  }

}
