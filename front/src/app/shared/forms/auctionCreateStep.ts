import { Output, EventEmitter } from '@angular/core';
export abstract class AuctionCreateStep<T> {
  @Output()
  outputOnStepComplete = new EventEmitter<T>();

  ready = false;
  titleMsg = '';

  completeStep(stepModel: T){
    this.outputOnStepComplete.emit(stepModel);
  }

  abstract onOkClick();
}
