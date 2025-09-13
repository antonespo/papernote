import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { Router } from '@angular/router';

import { NotesService } from '../services/notes.service';
import { AuthService } from '../../auth/services/auth.service';
import { NoteCardComponent } from './note-card.component';
import { NotesSearchComponent } from './notes-search.component';

@Component({
  selector: 'app-notes',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    NoteCardComponent,
    NotesSearchComponent,
  ],
  templateUrl: './notes.component.html',
  styleUrl: './notes.component.scss',
})
export class NotesComponent implements OnInit {
  private readonly notesService = inject(NotesService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  @ViewChild(NotesSearchComponent) searchComponent!: NotesSearchComponent;

  readonly isLoading = this.notesService.isLoading;
  readonly error = this.notesService.error;
  readonly notes = this.notesService.notes;
  readonly user = this.authService.user;

  readonly selectedTabIndex = signal(0);

  ngOnInit(): void {
    this.loadOwnedNotes();
  }

  onTabChange(index: number): void {
    this.selectedTabIndex.set(index);
    this.notesService.clearError();

    const filter = index === 0 ? 'owned' : 'shared';
    this.searchComponent?.setFilter(filter);

    if (index === 0) {
      this.loadOwnedNotes();
    } else {
      this.loadSharedNotes();
    }
  }

  onCreateNote(): void {
    this.router.navigate(['/notes', 'create']);
  }

  onRetry(): void {
    if (this.selectedTabIndex() === 0) {
      this.loadOwnedNotes();
    } else {
      this.loadSharedNotes();
    }
  }

  private loadOwnedNotes(): void {
    this.notesService.loadNotes('owned').subscribe();
  }

  private loadSharedNotes(): void {
    this.notesService.loadNotes('shared').subscribe();
  }

  clearError(): void {
    this.notesService.clearError();
  }
}
