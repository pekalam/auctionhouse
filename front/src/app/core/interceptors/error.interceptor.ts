import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpHeaders
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, empty, throwError, EMPTY } from 'rxjs';
import { flatMap, map, tap, catchError } from 'rxjs/operators';
import { AuthenticationStateService } from '../services/AuthenticationStateService';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';


@Injectable({
  providedIn: 'root'
})
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private router: Router) { }

  private handleError(err: HttpErrorResponse): boolean {
    switch (err.status) {
      case 500:
        this.router.navigateByUrl('/error', { state: { msg: 'Server error' } });
        break;
      case 504:
        this.router.navigateByUrl('/error', { state: { msg: 'Cannot connect to the server' } });
        break;
      case 404:
        this.router.navigateByUrl('/not-found');
        break;
      default:
        return false;
    }
    return true;
  }

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {


    return next.handle(req).pipe(catchError((err: HttpErrorResponse) => {
      console.log(err);
      if (this.handleError(err)) {
        return EMPTY;
      }
      return throwError(err);
    }));

  }
}
