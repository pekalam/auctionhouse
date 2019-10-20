import { Component, OnInit, Input } from '@angular/core';
import { CreateAuctionCommandArgs } from '../../../../../core/commands/CreateAuctionCommand';

@Component({
  selector: 'app-create-summary-step',
  templateUrl: './create-summary-step.component.html',
  styleUrls: ['./create-summary-step.component.scss']
})
export class CreateSummaryStepComponent implements OnInit {

  @Input("commandArgs")
  commandArgs: CreateAuctionCommandArgs;

  constructor() { }

  ngOnInit() {
  }

}
