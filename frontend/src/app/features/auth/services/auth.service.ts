import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of, throwError, EMPTY } from 'rxjs';
import { map, catchError, tap, finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthState } from '../../../shared/models/state.model';
import {
  AuthResponseDto,
  UserDto,
  LoginDto,
  RegisterDto,
  RefreshTokenDto,
  AuthService as AuthApiService,
} from '../../../api/auth';
import { extractErrorMessage } from '../../../shared/utils/error.utils';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);

  private readonly authState = signal<AuthState>({
    isLoading: false,
    error: null,
    isAuthenticated: false,
    user: null,
    accessToken: null,
    refreshToken: null,
    expiresAt: null,
  });

  readonly isLoading = computed(() => this.authState().isLoading);
  readonly error = computed(() => this.authState().error);
  readonly isAuthenticated = computed(() => this.authState().isAuthenticated);
  readonly user = computed(() => this.authState().user);
  readonly accessToken = computed(() => this.authState().accessToken);
  readonly refreshToken = computed(() => this.authState().refreshToken);
  readonly expiresAt = computed(() => this.authState().expiresAt);

  constructor() {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const token = localStorage.getItem('accessToken');
    const refreshToken = localStorage.getItem('refreshToken');
    const userJson = localStorage.getItem('user');
    const expiresAtString = localStorage.getItem('expiresAt');

    if (token && userJson) {
      try {
        const user = JSON.parse(userJson) as UserDto;
        const expiresAt = expiresAtString ? new Date(expiresAtString) : null;

        const isTokenExpired = expiresAt ? new Date() > expiresAt : false;

        if (isTokenExpired) {
          this.clearAuthState();
          return;
        }

        this.authState.update((state) => ({
          ...state,
          isAuthenticated: true,
          accessToken: token,
          refreshToken: refreshToken || null,
          user,
          expiresAt,
        }));
      } catch {
        this.logout();
      }
    }
  }

  login(usernameOrEmail: string, password: string): Observable<void> {
    const loginDto: LoginDto = {
      username: usernameOrEmail,
      password,
    };

    this.setLoading(true);
    this.clearError();

    return this.authApi.loginUser(loginDto).pipe(
      tap((response: AuthResponseDto) => {
        this.setAuthenticatedState(response);
      }),
      tap(() => {
        this.router.navigate(['/notes']);
      }),
      map(() => void 0),
      catchError((error: HttpErrorResponse) => {
        const errorMessage = extractErrorMessage(error);
        this.setError(errorMessage);
        this.setLoading(false);
        return throwError(() => error);
      }),
      finalize(() => {
        this.setLoading(false);
      })
    );
  }

  register(username: string, password: string): Observable<void> {
    const registerDto: RegisterDto = {
      username,
      password,
    };

    this.setLoading(true);
    this.clearError();

    return this.authApi.registerUser(registerDto).pipe(
      tap((response: AuthResponseDto) => {
        this.setAuthenticatedState(response);
      }),
      tap(() => {
        this.router.navigate(['/notes']);
      }),
      map(() => void 0),
      catchError((error: HttpErrorResponse) => {
        const errorMessage = extractErrorMessage(error);
        this.setError(errorMessage);
        this.setLoading(false);
        return throwError(() => error);
      }),
      finalize(() => {
        this.setLoading(false);
      })
    );
  }

  logout(): Observable<void> {
    const accessToken = this.accessToken();

    if (accessToken) {
      return this.authApi.logoutUser().pipe(
        tap(() => this.clearAuthState()),
        catchError(() => {
          this.clearAuthState();
          return of(void 0);
        })
      );
    } else {
      this.clearAuthState();
      return of(void 0);
    }
  }

  refreshAccessToken(): Observable<string | null> {
    const refreshToken = localStorage.getItem('refreshToken');

    if (!refreshToken) {
      return of(null);
    }

    const refreshTokenDto: RefreshTokenDto = {
      refreshToken,
    };

    return this.authApi.refreshToken(refreshTokenDto).pipe(
      tap((response: AuthResponseDto) => {
        this.setAuthenticatedState(response);
      }),
      map((response: AuthResponseDto) => response.accessToken || null),
      catchError(() => {
        this.clearAuthState();
        return of(null);
      })
    );
  }

  private clearAuthState(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('expiresAt');

    this.authState.set({
      isLoading: false,
      error: null,
      isAuthenticated: false,
      user: null,
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
    });

    this.router.navigate(['/auth/login']);
  }

  private setAuthenticatedState(response: AuthResponseDto): void {
    if (response.accessToken) {
      localStorage.setItem('accessToken', response.accessToken);
    }
    if (response.refreshToken) {
      localStorage.setItem('refreshToken', response.refreshToken);
    }
    if (response.user) {
      localStorage.setItem('user', JSON.stringify(response.user));
    }
    if (response.expiresAt) {
      localStorage.setItem('expiresAt', response.expiresAt);
    }

    const expiresAt = response.expiresAt ? new Date(response.expiresAt) : null;

    this.authState.update((state) => ({
      ...state,
      isAuthenticated: true,
      accessToken: response.accessToken || null,
      refreshToken: response.refreshToken || null,
      user: response.user || null,
      expiresAt,
      error: null,
    }));
  }

  isTokenExpired(): boolean {
    const expiresAt = this.expiresAt();
    return expiresAt ? new Date() > expiresAt : false;
  }

  isTokenExpiringSoon(minutesThreshold: number = 5): boolean {
    const expiresAt = this.expiresAt();
    if (!expiresAt) return false;

    const now = new Date();
    const thresholdTime = new Date(
      now.getTime() + minutesThreshold * 60 * 1000
    );
    return expiresAt <= thresholdTime;
  }

  getTimeUntilExpiration(): number | null {
    const expiresAt = this.expiresAt();
    if (!expiresAt) return null;

    return expiresAt.getTime() - new Date().getTime();
  }

  private setLoading(isLoading: boolean): void {
    this.authState.update((state) => ({ ...state, isLoading }));
  }

  private setError(error: string): void {
    this.authState.update((state) => ({ ...state, error }));
  }

  clearError(): void {
    this.authState.update((state) => ({ ...state, error: null }));
  }
}
