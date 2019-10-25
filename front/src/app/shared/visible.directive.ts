import { Directive, Input, ElementRef } from '@angular/core';

@Directive({
  selector: '[appVisible]'
})
export class VisibleDirective {
  private _visible: boolean = true;

  @Input('appVisible')
  set visible(v: boolean) {
    console.log(v);

    this._visible = v;
    if (this._visible === true) {
      this.el.nativeElement.style.visibility = 'hidden';
    } else {
      this.el.nativeElement.style.visibility = 'visible';
    }
  }

  constructor(private el: ElementRef) { }

}
