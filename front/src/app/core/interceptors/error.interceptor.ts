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
import { isDemoModeDisabled } from '../utils/DemoModeUtils';


@Injectable({
  providedIn: 'root'
})
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router, private loadingService: LoadingService) { }

  private handleError(err: HttpErrorResponse): boolean {
    switch (err.status) {
      case 500:
        this.router.navigateByUrl('/error', { state: { msg: 'Server error' } });
        break;
      case 504:
        this.router.navigateByUrl('/error', { state: { msg: 'Cannot connect to the server' } });
        break;
      case 503:
          this.router.navigateByUrl('/error', { state: { msg: 'Server is unavailable' } });
          break;
      case 404:
        this.router.navigateByUrl('/not-found');
        break;
      case 412:
        if(!isDemoModeDisabled()){
          this.router.navigateByUrl('/error', { state: { msg: 'Please disable demo mode at home page' } });
        }else{
          this.router.navigateByUrl('/error', { state: { msg: 'Server could not handle this request' } });
        }
        break;
      case 401:
        if(this.router.url != '/sign-in'){
          this.router.navigateByUrl('/sign-in', { state: { redirect: this.router.url } });
        }else{
          return false;
        }
        break;
      default:
        return false;
    }
    this.loadingService.resetLoading();
    return true;
  }

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if(req.url.endsWith("/api/c/demoCode") && err.status != 503){
        return throwError(err);
      }
      console.log(err);

      if(!this.handleError(err)){
        return throwError(err);
      }
    }));

  }
}
