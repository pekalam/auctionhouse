import { Component, OnInit } from '@angular/core';
import { BreakpointObserver, BreakpointState } from '@angular/cdk/layout';
import { MostViewedAuctionsQuery, MostViewedAuction } from '../../../core/queries/MostViewedAuctionsQuery';
import { Auction } from '../../../core/models/Auctions';
import { RecentlyViewedService } from '../../../core/services/RecentlyViewedService';
import { AuthenticationStateService } from '../../../core/services/AuthenticationStateService';
import { AuctionImageQuery } from '../../../core/queries/AuctionImageQuery';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Subject, Observable, empty, EMPTY } from 'rxjs';
import { debounceTime, switchMap, catchError, tap } from 'rxjs/operators';
import { HttpHeaders, HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss']
})
export class HomePageComponent implements OnInit {

  mostViewedAuctions: MostViewedAuction[];

  private codeValue = new Subject<string>();
  demoModeMessage: string = "";
  demoCodeForm: FormGroup = new FormGroup({
    code: new FormControl('', [Validators.required]),
  });
  showLoading = false;
  showCodeOk = false;
  showCodeInvalid = false;
  codeEnterSuccess = false;
  showDemoMode = true;
  showDemoAnim = false;

  constructor(private mostViewedAuctionsQuery: MostViewedAuctionsQuery, private httpClient: HttpClient) {
    this.mostViewedAuctionsQuery.execute().subscribe((result) => {
      this.mostViewedAuctions = result;
    });
    this.codeValue.pipe(
      debounceTime(500),
    ).subscribe(async v => {
      if(this.showLoading == true){
        return;
      }
      this.showLoading = true;
      this.showCodeOk = this.showCodeInvalid = false;
      await this.submitDemoCode(v)
    });
    if(document.cookie){
      const cookieMatch = document.cookie.match(/^.*demoModeDisabled\=true.*$/);
      this.showDemoMode = !(cookieMatch && cookieMatch.length > 0);      
    }
  }

  ngOnInit() {

  }

  private async submitDemoCode(demoCode: string){
    try{
      await this.httpClient.post("/api/c/demoCode", {demoCode}).toPromise()
      this.showCodeOk = true;
      this.codeEnterSuccess = true;
      document.getElementById('demo-container').addEventListener('animationend', () => {        
        this.showDemoMode = false;
      });
      this.showDemoAnim = true;

    }catch(error){
      console.log(error);
      this.showCodeInvalid = true;
    }    
    this.showLoading = false;
  }

  async onDemoCodeSubmit(){
    if (!this.codeEnterSuccess && this.demoCodeForm.controls.code.valid){
      await this.submitDemoCode(this.demoCodeForm.value.code)    
    }
  }

  onDemoCodeChange(){
    if (!this.codeEnterSuccess && this.demoCodeForm.controls.code.valid) {
      this.codeValue.next(this.demoCodeForm.value.code);
    }
  }
}
