import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/notes',
    pathMatch: 'full',
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full',
      },
      {
        path: 'login',
        loadComponent: () =>
          import('./features/auth/components/login.component').then(
            (m) => m.LoginComponent
          ),
      },
      {
        path: 'register',
        loadComponent: () =>
          import('./features/auth/components/register.component').then(
            (m) => m.RegisterComponent
          ),
      },
    ],
  },
  {
    path: 'notes',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/notes/components/notes.component').then(
            (m) => m.NotesComponent
          ),
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./features/notes/components/note-editor.component').then(
            (m) => m.NoteEditorComponent
          ),
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./features/notes/components/note-editor.component').then(
            (m) => m.NoteEditorComponent
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '/notes',
  },
];
