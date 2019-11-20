import * as signalR from '@aspnet/signalr';
import { Subject, Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { AuthenticationStateService } from './AuthenticationStateService';
import { distinctUntilChanged, map, first } from 'rxjs/operators';

export interface ServerMessage {
  result: string;
  correlationId: string;
  eventName: string;
  values?: any;
}

@Injectable({
  providedIn: 'root'
})
export class ServerMessageService {
  private connection: signalR.HubConnection;
  private handlerMap = new Map<string, Subject<ServerMessage>>();
  private connectionStartedSubj = new Subject<boolean>();

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

    this.connection.on('completed', (eventName: string, correlationId: string, values?: object) =>
      this.handleServerMessage({ result: 'completed', eventName, correlationId, values }));

    this.connection.on('failure', (eventName: string, correlationId: string, values?: object) =>
      this.handleServerMessage({ result: 'failure', eventName, correlationId, values }));


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

  private handleServerMessage(serverMessage: ServerMessage) {
    console.log('handling ' + serverMessage.eventName);
    console.log('result: ' + serverMessage.result);
    console.log('correlationid: ' + serverMessage.correlationId);


    if (this.handlerMap.has(serverMessage.eventName)) {
      const handler = this.handlerMap.get(serverMessage.eventName);
      handler.next(serverMessage);
    } else {
      throw new Error('no handler for ' + serverMessage.eventName);
    }
  }

  setupServerMessageHandler(eventName: string): Observable<ServerMessage> {
    console.log('registered handler for ' + eventName);

    const newSubj = new Subject<ServerMessage>();
    this.handlerMap.set(eventName, newSubj);
    return newSubj.asObservable().pipe(first());
  }
}
