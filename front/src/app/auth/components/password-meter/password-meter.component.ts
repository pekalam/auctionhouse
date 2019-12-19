import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { PasswordStrength } from '../../../core/utils/PasswordStrength';

@Component({
  selector: 'app-password-meter',
  templateUrl: './password-meter.component.html',
  styleUrls: ['./password-meter.component.scss']
})
export class PasswordMeterComponent implements OnInit {

  @Input()
  set password(value: string) {
    if (value) {
      this.passwordStrength = PasswordStrength.measure(value);
    } else {
      this.passwordStrength = null;
    }
    this.strengthChange.emit(this.passwordStrength);
  }

  @Output()
  strengthChange = new EventEmitter<string>();

  passwordStrength = null;

  constructor() { }

  ngOnInit() {
  }

}
