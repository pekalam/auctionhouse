import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { CategoriesQuery } from '../../../core/queries/CategoriesQuery';
import { CreateAuctionCommandArgs, CreateAuctionCommand } from '../../../core/commands/CreateAuctionCommand';
import { CategoryTreeNode } from '../../../core/models/CategoryTreeNode';
import { Product } from '../../../core/models/Product';
import { CategorySelectStep } from '../../categorySelectStep';
import { ProductStep } from '../../productStep';
import { ImgUploadResult } from '../../components/img-upload-input/img-upload-input.component';
import { Router } from '@angular/router';
import { StartAuctionCreateSessionCommand } from '../../../core/commands/StartAuctionCreateSessionCommand';
import { ServerMessageService } from '../../../core/services/ServerMessageService';
import { Subscription } from 'rxjs';
import { first } from 'rxjs/operators';

@Component({
  selector: 'app-auction-create-page',
  templateUrl: './auction-create-page.component.html',
  styleUrls: ['./auction-create-page.component.scss']
})
export class AuctionCreatePageComponent implements OnInit, OnDestroy{
  private totalSteps: number = 4;
  private categorySelectStep: CategorySelectStep;
  private productStep: ProductStep;
  private connectionStartedSub: Subscription;

  createAuctionArgs: CreateAuctionCommandArgs;
  step = 0;
  error: string = null;
  canMoveForward: boolean = false;
  showCreateForm = false;

  constructor(private startAuctionCreateSessionCommand: StartAuctionCreateSessionCommand,
              private createAuctionCommand: CreateAuctionCommand,
              private serverMessageService: ServerMessageService,
              private router: Router) {
  }

  ngOnInit() {
    this.connectionStartedSub = this.serverMessageService.connectionStarted.subscribe((connected) => {
      if (connected) {
        this.startAuctionCreateSession();
      } else {
        this.router.navigate(['/home']);
      }
    });
    this.serverMessageService.ensureConnected();
  }

  ngOnDestroy(): void {
    this.connectionStartedSub.unsubscribe();
  }

  private startAuctionCreateSession() {
    console.log("starting session");
    this.startAuctionCreateSessionCommand.execute().subscribe((msg) => {
      if (msg.result === 'completed') {
        console.log('Create session started');
        this.showCreateForm = true;
      } else {
        console.log('Cannot start create session');
        this.showCreateForm = false;
      }
    }, (err) => {
      console.log('Cannot start create session');
    });
  }

  private createCommandArgs() {
    let categories = [this.categorySelectStep.selectedMainCategory.categoryName,
    this.categorySelectStep.selectedSubCategory.categoryName,
    this.categorySelectStep.selectedSubCategory2.categoryName];
    this.createAuctionArgs = new CreateAuctionCommandArgs(this.productStep.buyNowPrice, this.productStep.startDate,
      this.productStep.endDate, categories, '123', this.productStep.product);
  }

  onCategorySelectedStep(stepResult: CategorySelectStep) {
    this.categorySelectStep = stepResult;
    this.step++;
    this.canMoveForward = true;
  }

  onProductStep(stepResult: ProductStep) {
    this.productStep = stepResult;
    this.step++;
  }

  onImagesStep(stepResult: ImgUploadResult[]) {
    console.log('img step = ');
    console.log(stepResult);
    this.createCommandArgs();
    console.log(this.createAuctionArgs);

    this.step++;
  }

  onForwardClick() {
    if (this.step + 1 < this.totalSteps) {
      this.step++;
    }
  }

  onBackClick() {
    if (this.step - 1 >= 0) {
      this.step--;
    }
  }

  onCreateClick() {
    this.createAuctionCommand.execute(this.createAuctionArgs).subscribe((msg) => {
      if (msg.result === 'completed') {
        this.router.navigate(['/user']);
      } else {
        this.error = 'Cannot create an auction'
      }
    }, (err) => {
      this.error = 'Cannot create an auction'
    });
  }
}
