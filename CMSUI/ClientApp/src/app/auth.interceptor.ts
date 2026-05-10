import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const raw = sessionStorage.getItem('usuario');
    if (raw) {
      const token = JSON.parse(raw).token;
      if (token) {
        return next.handle(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }))
          .pipe(catchError(err => {
            if (err.status === 401) {
              sessionStorage.removeItem('usuario');
              sessionStorage.removeItem('adminTenant');
              window.location.href = '/login';
            }
            return throwError(() => err);
          }));
      }
    }
    return next.handle(req);
  }
}
