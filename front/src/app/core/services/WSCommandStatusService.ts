import * as signalR from '@aspnet/signalr';
import { Subject, Observable, of } from 'rxjs';
import { Injectable } from '@angular/core';
import { AuthenticationStateService } from './AuthenticationStateService';
import { distinctUntilChanged, map, first } from 'rxjs/operators';

export interface RequestStatus {
  correlationId: string;
  status: string;
  extraData?: any;
}

@Injectable({
  providedIn: 'root'
})
export class WSCommandStatusService {
  private connection: signalR.HubConnection;
  private handlerMap = new Map<string, Subject<RequestStatus>>();
  private connectionStartedSubj = new Subject<boolean>();
  private unhandledMesssages = new Array<RequestStatus>();

  connectionStarted: Observable<boolean> = this.connectionStartedSubj;

  constructor(private authenticationService: AuthenticationStateService) {
    this.authenticationService.isAuthenticated.subscribe((isAuth) => {
      if (!isAuth) {
        this.closeConnection();
      }
    });
  }

  ensureConnected() {
    if (this.authenticationService.checkIsAuthenticated()) {
      const jwt = localStorage.getItem('user');
      console.log('connecting..');
      this.initHubConnection(jwt);
    } else {
      throw new Error('Cannot connect to server - user is not authenticated');
    }
  }

  private initHubConnection(jwt: string) {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      console.log("already connected");

      this.connectionStartedSubj.next(true);
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`http://localhost:5000/app?token=${jwt}`)
      .build();

    this.connection.onclose((err) => {
      console.log('connection closed by server ');
      console.log(err);
      this.connectionStartedSubj.next(false);
    });

    this.connection.on('completed', (requestStatus: RequestStatus) =>
      this.handleServerMessage(requestStatus));

    this.connection.on('failed', (requestStatus: RequestStatus) =>
      this.handleServerMessage(requestStatus));


    this.connection.start().then(() => {
      console.log('connection initialized');
      this.connectionStartedSubj.next(true);
    }).catch(err => {
      this.connectionStartedSubj.next(false);
      console.log(err);
    });
  }

  private closeConnection() {
    if (this.connection) {
      this.connection.stop().then(() => {
        console.log('connection closed');
        this.connection = null;
      });
    }
  }

  private handleServerMessage(serverMessage: RequestStatus) {
    console.log(serverMessage);

    console.log('handling ' + serverMessage.correlationId);
    console.log('status: ' + serverMessage.status);
    console.log('extraData: ' + serverMessage.extraData);


    if (this.handlerMap.has(serverMessage.correlationId)) {
      const handler = this.handlerMap.get(serverMessage.correlationId);
      handler.next(serverMessage);
    } else {
      this.unhandledMesssages.push(serverMessage);
    }
  }

  setupServerMessageHandler(correlationId: string): Observable<RequestStatus> {
    console.log('registered handler for ' + correlationId);

    let unhandled = this.unhandledMesssages.filter(m => m.correlationId === correlationId);
    if (unhandled.length > 0) {
      this.unhandledMesssages = this.unhandledMesssages.filter(m => m.correlationId !== correlationId);
      return of(unhandled[0]);
    }

    const newSubj = new Subject<RequestStatus>();
    this.handlerMap.set(correlationId, newSubj);
    return newSubj.asObservable().pipe(first());
  }
}
