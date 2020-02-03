import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'dateToTimer'
})
export class DateToTimerPipe implements PipeTransform {

  transform(value: Date, ...args: any[]): any {
    let h = value.getHours();
    let m = value.getMinutes();
    let s = value.getSeconds();

    return `${h < 10 ? `0${h}` : h}:${m < 10 ? `0${m}` : m}:${s < 10 ? `0${s}` : s}`;
  }

}
