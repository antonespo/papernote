import {
  Component,
  inject,
  computed,
  signal,
  OnInit,
  OnDestroy,
  effect,
} from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { NavbarComponent } from './shared/components/navbar.component';
import { AuthService } from './features/auth/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, NavbarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly destroy$ = new Subject<void>();

  readonly isAuthenticated = this.authService.isAuthenticated;
  private readonly currentRoute = signal('');

  readonly showNavbar = computed(() => {
    const authenticated = this.isAuthenticated();
    const route = this.currentRoute();
    const isAuthPage = route.includes('/auth/');
    return authenticated && !isAuthPage;
  });

  title = 'papernote-ui';

  ngOnInit(): void {
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe((event: NavigationEnd) => {
        this.currentRoute.set(event.url);
      });

    effect(() => {
      const isAuthenticated = this.isAuthenticated();
      const currentUrl = this.currentRoute();

      if (
        !isAuthenticated &&
        currentUrl &&
        !currentUrl.includes('/auth/login') &&
        !currentUrl.includes('/auth/register')
      ) {
        setTimeout(() => {
          this.router.navigate(['/auth/login']);
        }, 0);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
