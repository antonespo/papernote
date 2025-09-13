import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';

import { AuthService } from '../../features/auth/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <mat-toolbar color="primary" class="app-navbar">
      <div class="navbar-content">
        <div class="navbar-brand" (click)="navigateToNotes()">
          <mat-icon class="brand-icon">note</mat-icon>
          <span class="brand-text">PaperNote</span>
        </div>

        <div class="navbar-actions">
          @if (user(); as currentUser) {
          <button mat-button [matMenuTriggerFor]="userMenu" class="user-button">
            <mat-icon>account_circle</mat-icon>
            <span class="username">{{ currentUser.username }}</span>
            <mat-icon>arrow_drop_down</mat-icon>
          </button>

          <mat-menu #userMenu="matMenu" class="user-menu">
            <button
              mat-menu-item
              (click)="onLogout()"
              class="logout-item"
              [disabled]="isLoggingOut()"
            >
              @if (isLoggingOut()) {
              <mat-spinner diameter="16"></mat-spinner>
              } @else {
              <mat-icon>logout</mat-icon>
              }
              <span>Logout</span>
            </button>
          </mat-menu>
          }
        </div>
      </div>
    </mat-toolbar>
  `,
  styles: [
    `
      .app-navbar {
        position: sticky;
        top: 0;
        z-index: 1000;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

        &.mat-primary {
          background-color: #1976d2;
          color: white;
        }
      }

      .navbar-content {
        display: flex;
        justify-content: space-between;
        align-items: center;
        width: 100%;
        max-width: 1200px;
        margin: 0 auto;
        padding: 0 1rem;
      }

      .navbar-brand {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        cursor: pointer;
        transition: opacity 0.2s ease;
        color: white;

        &:hover {
          opacity: 0.8;
        }

        .brand-icon {
          font-size: 1.5rem;
          width: 1.5rem;
          height: 1.5rem;
          color: white;
        }

        .brand-text {
          font-size: 1.25rem;
          font-weight: 600;
          letter-spacing: 0.025em;
          color: white;
        }
      }

      .navbar-actions {
        display: flex;
        align-items: center;
      }

      .user-button {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        color: white !important;
        background-color: rgba(255, 255, 255, 0.1);
        border-radius: 8px;
        padding: 0.5rem 1rem;
        transition: background-color 0.2s ease;

        &:hover {
          background-color: rgba(255, 255, 255, 0.2);
        }

        .username {
          font-weight: 500;
          max-width: 200px;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
          color: white;
        }

        mat-icon {
          color: white;
        }

        @media (max-width: 480px) {
          .username {
            display: none;
          }
        }
      }

      .user-menu {
        margin-top: 0.5rem;

        .mat-mdc-menu-panel {
          min-width: 160px;
        }
      }

      .logout-item {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        color: #d32f2f !important;
        transition: background-color 0.2s ease;

        &:hover:not(:disabled) {
          background-color: rgba(211, 47, 47, 0.1);
        }

        &:disabled {
          opacity: 0.6;
          cursor: not-allowed;
        }

        mat-icon {
          color: #d32f2f;
        }

        mat-spinner {
          color: #d32f2f;
        }
      }
    `,
  ],
})
export class NavbarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.authService.user;
  readonly isLoggingOut = signal(false);

  navigateToNotes(): void {
    this.router.navigate(['/notes']);
  }

  onLogout(): void {
    this.isLoggingOut.set(true);

    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      },
      error: (error) => {
        console.error('Logout error:', error);
        this.router.navigate(['/auth/login']);
      },
      complete: () => {
        this.isLoggingOut.set(false);
      },
    });
  }
}
