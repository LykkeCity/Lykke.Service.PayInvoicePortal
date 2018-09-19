import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ROUTE_SIGNIN_PAGE } from '../constants/routes';
import { environment } from '../../environments/environment';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError(err => {
        if (err.status === 401 && environment.production) {
          const path = location.pathname;

          location.href =
            ROUTE_SIGNIN_PAGE + path && path.length && path !== '/' ? '?ReturnUrl=' + path : '';
        }

        return throwError(err);
      })
    );
  }
}
