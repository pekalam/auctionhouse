import { Injectable } from '@angular/core';
import { RequestStatus, WSCommandStatusService } from '../../services/WSCommandStatusService';
import { Observable, of } from 'rxjs';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { filter, switchMap, map, catchError } from 'rxjs/operators';
import { CommandHelper, ResponseOptions } from '../ComandHelper';
import { environment } from '../../../../environments/environment';



@Injectable({
  providedIn: 'root'
})
export class AddAuctionImageCommand {
  constructor(private httpClient: HttpClient, private commandHelper: CommandHelper) {
  }

  execute(event: FileList, imgNum: number): Observable<RequestStatus> {
    const url = `${environment.API_URL}/api/addAuctionImage`;
    const file: File = event && event.item(0);
    const formData = new FormData();
    formData.append('img', file);
    formData.append('img-num', imgNum.toString());
    const httpHeaders = new HttpHeaders({ 'enctype': 'multipart/form-data' });

    const req = this.httpClient.post(url, formData, { headers: httpHeaders });
    return this.commandHelper.getResponseStatusHandler(req, true, ResponseOptions.HTTPQueuedCommand, {
      intervalSec: 2000, maxRetry: 5
    });
  }
}
