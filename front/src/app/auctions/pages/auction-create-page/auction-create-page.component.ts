import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Location } from '@angular/common';
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
export class AuctionCreatePageComponent implements OnInit, OnDestroy {
  private totalSteps: number = 4;
  private categorySelectStep: CategorySelectStep;
  private productStep: ProductStep;
  private connectionStartedSub: Subscription;

  @ViewChild("categoryStep", { static: true })
  categoryStepComponent;
  @ViewChild("productStep", { static: true })
  productStepComponent;
  @ViewChild("imageStep", { static: true })
  imageStepComponent;
  @ViewChild("summaryStep", { static: true })
  summaryStepComponent;

  createAuctionArgs: CreateAuctionCommandArgs;
  step = 1;
  error: string = null;
  canMoveForward = false;
  showCreateForm = false;
  showOk = false;
  formMessage: string;

  constructor(private startAuctionCreateSessionCommand: StartAuctionCreateSessionCommand,
    private createAuctionCommand: CreateAuctionCommand,
    private serverMessageService: ServerMessageService,
    private router: Router,
    public location: Location) {
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
    this.changeFormMessage();
    this.checkIsComponentReady();
  }

  ngOnDestroy(): void {
    this.connectionStartedSub.unsubscribe();
  }

  private nextStep() {
    this.step++;
    this.showOk = false;
    this.changeFormMessage();
    this.checkIsComponentReady();
  }

  private previousStep() {
    this.step--;
    this.showOk = false;
    this.changeFormMessage();
    this.checkIsComponentReady()
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

  private changeFormMessage() {
    switch (this.step) {
      case 0:
        this.formMessage = "Create new auction";
        break;
      case 1:
        this.formMessage = "Type auction data";
        break;
      case 2:
        this.formMessage = "Add image";
        break;
      case 3:
        this.formMessage = "Summary"
        break;
      default:
        break;
    }
  }

  private checkIsComponentReady(){
    switch (this.step) {
      case 0:
        this.categoryStepComponent.checkIsReady();
        break;
      case 1:
        this.productStepComponent.checkIsReady();
        break;
      case 2:
        this.imageStepComponent.checkIsReady();
        break;
      case 3:
        this.summaryStepComponent.checkIsReady();
        break;
      default:
        break;
    }
  }

  onCategorySelectedStep(stepResult: CategorySelectStep) {
    this.categorySelectStep = stepResult;
    this.nextStep();
  }

  onProductStep(stepResult: ProductStep) {
    this.productStep = stepResult;
    this.nextStep();
  }

  onImagesStep(stepResult: ImgUploadResult[]) {
    console.log('img step = ');
    console.log(stepResult);
    this.createCommandArgs();
    console.log(this.createAuctionArgs);

    this.nextStep();
  }

  onForwardClick() {
    if (this.step + 1 < this.totalSteps) {
      this.nextStep();
    }
  }

  onBackClick() {
    if (this.step - 1 >= 0) {
      this.previousStep();
    }
  }

  onOkClick() {
    switch (this.step) {
      case 0:
        this.categoryStepComponent.onOkClick();
        break;
      case 1:
        this.productStepComponent.onOkClick();
        break;
      case 2:
        this.imageStepComponent.onOkClick();
        break;
      case 3:
        this.summaryStepComponent.onOkClick();
        break;
      default:
        break;
    }
  }

  onSummaryStep() {
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
