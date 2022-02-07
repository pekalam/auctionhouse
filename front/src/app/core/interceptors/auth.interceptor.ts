import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpHeaders
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, empty, throwError, EMPTY } from 'rxjs';
import { flatMap, map, tap, catchError, filter } from 'rxjs/operators';
import { AuthenticationStateService } from '../services/AuthenticationStateService';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { LoadingService } from '../services/LoadingService';


@Injectable({
  providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authStateService: AuthenticationStateService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const authHeader = null;//this.authStateService.getAuthorizationHttpHeader();

/*     if (authHeader) {
      const request = req.clone({
        setHeaders: {
          Authorization: authHeader
        }
      });
      return next.handle(request);
    } */

    return next.handle(req).pipe(
      //@ts-ignore
      tap((h) => {
        console.log(h);
        
      })
      ,catchError((err: HttpErrorResponse) => {

      if(err.status == 401){
        console.log('Token is probably expired');
        this.authStateService.removeLocalAuthData();
        if(req.url == "/api/c/signout"){ //expired token when trying to signout
          return;
        }
      }
      return throwError(err);
    })); 

    

  }
}
