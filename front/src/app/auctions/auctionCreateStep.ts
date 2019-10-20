import { Output, EventEmitter } from '@angular/core';
export class AuctionCreateStep<T> {
  @Output()
  outputOnStepComplete = new EventEmitter<T>();

  completeStep(stepModel: T){
    this.outputOnStepComplete.emit(stepModel);
  }
}
