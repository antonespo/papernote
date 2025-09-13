import { Injectable, inject } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject, EMPTY } from 'rxjs';
import { catchError, switchMap, filter, take } from 'rxjs/operators';
import { AuthService } from '../../features/auth/services/auth.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  private readonly authService = inject(AuthService);
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const accessToken = this.authService.accessToken();
    const isAuthEndpoint = this.isAuthEndpoint(request.url);
    const isLogoutEndpoint = this.isLogoutEndpoint(request.url);

    // Add JWT token for protected endpoints (Notes API and logout)
    if (accessToken && (!isAuthEndpoint || isLogoutEndpoint)) {
      request = this.addTokenToRequest(request, accessToken);
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        // Handle 401 Unauthorized for token refresh
        if (error.status === 401 && accessToken && !isAuthEndpoint) {
          return this.handle401Error(request, next);
        }
        return throwError(() => error);
      })
    );
  }

  private addTokenToRequest(
    request: HttpRequest<unknown>,
    token: string
  ): HttpRequest<unknown> {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  private isAuthEndpoint(url: string): boolean {
    return (
      url.includes('/api/v1/auth/') &&
      !url.includes('/api/v1/auth/logout') &&
      !url.includes('/api/v1/auth/refresh')
    );
  }

  private isLogoutEndpoint(url: string): boolean {
    return url.includes('/api/v1/auth/logout');
  }

  private handle401Error(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        return this.authService.refreshAccessToken().pipe(
          switchMap((newToken: string | null) => {
            this.isRefreshing = false;
            if (newToken) {
              this.refreshTokenSubject.next(newToken);
              return next.handle(this.addTokenToRequest(request, newToken));
            } else {
              this.authService.logout();
              return EMPTY;
            }
          }),
          catchError((error) => {
            this.isRefreshing = false;
            this.authService.logout();
            return throwError(() => error);
          })
        );
      } else {
        this.isRefreshing = false;
        this.authService.logout();
        return EMPTY;
      }
    }

    // Wait for refresh to complete
    return this.refreshTokenSubject.pipe(
      filter((token) => token !== null),
      take(1),
      switchMap((token) => next.handle(this.addTokenToRequest(request, token!)))
    );
  }
}
