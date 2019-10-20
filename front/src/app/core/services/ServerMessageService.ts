import * as signalR from '@aspnet/signalr';
import { Subject, Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { AuthenticationStateService } from './AuthenticationStateService';
import { distinct } from 'rxjs/operators';

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

  connectionStarted: Observable<boolean> = this.connectionStartedSubj.pipe(distinct());

  constructor(private authenticationService: AuthenticationStateService) {
    authenticationService.isAuthenticated.subscribe((isAuth) => {
      if (isAuth) {
        const jwt = localStorage.getItem('user');
        this.initConnection(jwt);
      } else {
        this.closeConnection();
      }
    });
  }

  private initHubConnection(jwt: string) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`http://localhost:5000/app?token=${jwt}`)
      .build();

    this.connection.start().then(() => {
      console.log('connection initialized');
      this.connectionStartedSubj.next(true);
    }).catch(err => {
      this.connectionStartedSubj.next(false);
      console.log(err);
    });

    this.connection.on('completed', (eventName: string, correlationId: string, values?: object) =>
      this.handleServerMessage({ result: 'completed', eventName, correlationId, values }));

    this.connection.on('failure', (eventName: string, correlationId: string, values?: object) =>
      this.handleServerMessage({ result: 'failure', eventName, correlationId, values }));
  }

  initConnection(jwt: string) {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      this.connection.stop().then(() => {
        this.initHubConnection(jwt);
      })
    } else {
      this.initHubConnection(jwt);
    }
  }

  private closeConnection() {
    if (this.connection) {
      this.connection.stop().then(() => {
        console.log('connection closed');
      });
    }
  }

  private handleServerMessage(serverMessage: ServerMessage) {
    console.log('handling ' + serverMessage.eventName);

    if (this.handlerMap.has(serverMessage.eventName)) {
      const handler = this.handlerMap.get(serverMessage.eventName);
      handler.next(serverMessage);
    } else {
      console.log('no handler for ' + serverMessage.eventName);
    }
  }

  setupServerMessageHandler(eventName: string): Observable<ServerMessage> {
    console.log('registered handler for ' + eventName);

    const newSubj = new Subject<ServerMessage>();
    this.handlerMap.set(eventName, newSubj);
    return newSubj.asObservable();
  }
}
