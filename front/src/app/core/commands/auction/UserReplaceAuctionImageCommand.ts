import { Injectable } from '@angular/core';
import { RequestStatus, WSCommandStatusService } from '../../services/WSCommandStatusService';
import { Observable, of } from 'rxjs';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { filter, catchError, switchMap } from 'rxjs/operators';
import { WSCommandHelper } from '../WSCommandHelper';
import { CommandHelper, ResponseOptions } from '../ComandHelper';
import { environment } from '../../../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class UserReplaceAuctionImageCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(event: FileList, imgNum: number, auctionId: string): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/c/userReplaceAuctionImage`;
    const file: File = event && event.item(0);
    const formData = new FormData();
    formData.append('img', file);
    formData.append('img-num', imgNum.toString());
    formData.append('auction-id', auctionId);
    console.log(formData);

    const httpHeaders = new HttpHeaders({ 'enctype': 'multipart/form-data' });
    const req = this.httpClient.post(url, formData, { headers: httpHeaders });
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.WSQueuedCommand);
  }
}
