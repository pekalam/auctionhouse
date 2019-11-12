import { Component, OnInit, Input } from '@angular/core';
import { CreateAuctionCommandArgs } from '../../../../../core/commands/CreateAuctionCommand';
import { AuctionCreateStep } from '../../../../auctionCreateStep';

@Component({
  selector: 'app-create-summary-step',
  templateUrl: './create-summary-step.component.html',
  styleUrls: ['./create-summary-step.component.scss']
})
export class CreateSummaryStepComponent extends AuctionCreateStep<void> implements OnInit {

  titleMsg = 'Summary';

  @Input("commandArgs")
  commandArgs: CreateAuctionCommandArgs;

  constructor() { super(); this.ready = true; }

  ngOnInit() {
  }

  onOkClick() {
    this.outputOnStepComplete.emit();
  }

}
