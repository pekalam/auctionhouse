import { HttpClient } from '@angular/common/http';
import { RequestStatus } from '../../services/WSCommandStatusService';
import { Injectable } from '@angular/core';
import { CommandHelper } from '../ComandHelper';
import { environment } from '../../../../environments/environment';
import { map, tap } from 'rxjs/operators';



@Injectable({
  providedIn: 'root'
})
export class SignOutCommand {

  constructor(private httpClient: HttpClient) {
  }

  execute(): Promise<{}> {
    const req = this.httpClient.post(`${environment.API_URL}/api/c/signout`, null);
    return req.toPromise();
  }
}
