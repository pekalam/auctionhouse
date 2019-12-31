import { Component, OnInit } from '@angular/core';
import { UserDataQuery, UserData } from '../../../../../core/queries/UserDataQuery';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { ChangePasswordCommand } from '../../../../../core/commands/auth/ChangePasswordCommand';
import { LoadingService } from '../../../../../core/services/LoadingService';


@Component({
  selector: 'app-user-data-page',
  templateUrl: './user-data-page.component.html',
  styleUrls: ['./user-data-page.component.scss']
})
export class UserDataPageComponent implements OnInit {

  userData: UserData;
  changePasswordForm = new FormGroup({
    password: new FormControl('', [Validators.required, Validators.minLength(4)]),
    confirm: new FormControl('', [Validators.required, Validators.minLength(4)])
  });
  editPassword = false;
  passwordsMatch = true;

  constructor(userDataQuery: UserDataQuery, private changePasswordCommand: ChangePasswordCommand, private loadingService: LoadingService) {
    userDataQuery.execute().subscribe((data) => {
      this.userData = data;
    });
  }

  onConfirmType() {
    if (this.changePasswordForm.value.confirm !== this.changePasswordForm.value.password) {
      this.passwordsMatch = false;
      this.changePasswordForm.controls.confirm.setErrors({ match: false });
    } else {
      this.passwordsMatch = true;
      this.changePasswordForm.controls.confirm.setErrors(null);
    }
  }

  onPasswordChangeFormSubmit() {
    if (this.changePasswordForm.valid && this.passwordsMatch) {
      this.loadingService.setLoading(true);
      this.changePasswordCommand.execute(this.changePasswordForm.value.password)
      .subscribe((v) => {
        this.loadingService.setLoading(false);
        this.editPassword = false;
      }, (err) => console.log(err));
    }
  }

  ngOnInit() {
  }

}
