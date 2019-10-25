import { Output, EventEmitter } from '@angular/core';
export abstract class AuctionCreateStep<T> {
  @Output()
  outputOnStepComplete = new EventEmitter<T>();

  @Output()
  onStepReady = new EventEmitter<void>();

  completeStep(stepModel: T){
    this.outputOnStepComplete.emit(stepModel);
  }

  abstract onOkClick();

  abstract checkIsReady();
}
