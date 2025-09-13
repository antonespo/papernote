import { inject } from '@angular/core';
import {
  HttpInterceptorFn,
  HttpRequest,
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
} from '@angular/common/http';
import { catchError, switchMap, filter, take } from 'rxjs/operators';
import { throwError, BehaviorSubject, EMPTY, Observable } from 'rxjs';
import { AuthService } from '../../features/auth/services/auth.service';

let isRefreshing = false;
let refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const jwtInterceptor: HttpInterceptorFn = (
  req,
  next
): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);
  const accessToken = authService.accessToken();
  const isAuthEndpoint = checkIsAuthEndpoint(req.url);
  const isLogoutEndpoint = checkIsLogoutEndpoint(req.url);

  if (accessToken && (!isAuthEndpoint || isLogoutEndpoint)) {
    req = addTokenToRequest(req, accessToken);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && accessToken && !isAuthEndpoint) {
        return handle401Error(req, next, authService);
      }
      return throwError(() => error);
    })
  );
};

function addTokenToRequest(
  request: HttpRequest<unknown>,
  token: string
): HttpRequest<unknown> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

function checkIsAuthEndpoint(url: string): boolean {
  return (
    url.includes('/api/v1/auth/') &&
    !url.includes('/api/v1/auth/logout') &&
    !url.includes('/api/v1/auth/refresh')
  );
}

function checkIsLogoutEndpoint(url: string): boolean {
  return url.includes('/api/v1/auth/logout');
}

function handle401Error(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService
): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      return authService.refreshAccessToken().pipe(
        switchMap((newToken: string | null) => {
          isRefreshing = false;
          if (newToken) {
            refreshTokenSubject.next(newToken);
            return next(addTokenToRequest(request, newToken));
          } else {
            authService.logout();
            return EMPTY;
          }
        }),
        catchError((error) => {
          isRefreshing = false;
          authService.logout();
          return throwError(() => error);
        })
      );
    } else {
      isRefreshing = false;
      authService.logout();
      return EMPTY;
    }
  }

  return refreshTokenSubject.pipe(
    filter((token) => token !== null),
    take(1),
    switchMap((token) => next(addTokenToRequest(request, token!)))
  );
}
