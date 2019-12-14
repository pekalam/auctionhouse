import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpHeaders
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { flatMap, map } from 'rxjs/operators';
import { AuthenticationStateService } from '../services/AuthenticationStateService';


@Injectable({
  providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authStateService: AuthenticationStateService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const authHeader = this.authStateService.getAuthorizationHttpHeader();

    if (authHeader) {
      const request = req.clone({
        setHeaders: {
          Authorization: authHeader
        }
      });
      return next.handle(request);
    }
    return next.handle(req);

  }
}
