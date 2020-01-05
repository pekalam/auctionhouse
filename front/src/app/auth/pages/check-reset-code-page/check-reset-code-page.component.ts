import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { CheckResetCodeCommand } from '../../../core/commands/auth/CheckResetCodeCommand';
import { Router } from '@angular/router';

@Component({
  selector: 'app-check-reset-code-page',
  templateUrl: './check-reset-code-page.component.html',
  styleUrls: ['./check-reset-code-page.component.scss']
})
export class CheckResetCodePageComponent implements OnInit {

  emailArg = '';
  form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    resetCode: new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]),
  });

  constructor(private checkResetCodeCommand: CheckResetCodeCommand, private router: Router) {
    if (this.router.getCurrentNavigation().extras.state) {
      this.emailArg = this.router.getCurrentNavigation().extras.state.email;
      this.form.controls.email.setValue(this.emailArg);
    }
  }

  ngOnInit() {
  }

  onFormSubmit() {
    if (this.form.valid) {
      this.checkResetCodeCommand.execute(this.form.value.resetCode, this.form.value.email).subscribe((msg) => {
        if (msg.status === 'COMPLETED') {
          this.router.navigate(['/reset-password'], { state: this.form.value });
        } else {
          this.router.navigateByUrl('/error');
        }
      });
    }
  }
}
