import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { RequestStatus, WSCommandStatusService } from '../../services/WSCommandStatusService';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { Product } from '../../models/Product';
import { WSCommandHelper } from '../WSCommandHelper';
import { environment } from '../../../../environments/environment';
import { CommandHelper, ResponseOptions } from '../ComandHelper';

export interface CreateAuctionCommandArgs {
  buyNowPrice: number | null;
  startDate: Date;
  endDate: Date;
  category: Array<string>;
  product: Product;
  tags: string[];
  name: string;
  buyNowOnly: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CreateAuctionCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(commandArgs: CreateAuctionCommandArgs): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/createAuction`;
    console.log(commandArgs);

    const req = this.httpClient.post(url, commandArgs);
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand, {intervalSec: 1500, maxRetry: 4});
  }
}
