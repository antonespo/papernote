import { Component, Input, inject, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { NoteSummaryDto } from '../../../api/notes';
import { NotesService } from '../services/notes.service';
import { extractErrorMessage } from '../../../shared/utils/error.utils';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog.component';

@Component({
  selector: 'app-note-card',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
  ],
  templateUrl: './note-card.component.html',
  styleUrl: './note-card.component.scss',
})
export class NoteCardComponent {
  private readonly router = inject(Router);
  private readonly notesService = inject(NotesService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  @Input({ required: true }) note!: NoteSummaryDto;
  @Input() isOwned: boolean = true;
  @Output() noteDeleted = new EventEmitter<string>();

  onNoteClick(): void {
    if (this.isOwned) {
      this.router.navigate(['/notes', this.note.id]);
    } else {
      this.router.navigate(['/notes', this.note.id], {
        queryParams: { readonly: 'true' },
      });
    }
  }

  onEditClick(event: Event): void {
    event.stopPropagation();
    if (this.isOwned) {
      this.router.navigate(['/notes', this.note.id]);
    }
  }

  onDeleteClick(event: Event): void {
    event.stopPropagation();

    const dialogData: ConfirmDialogData = {
      title: 'Delete Note',
      message: `Are you sure you want to delete "${this.note.title}"? This action cannot be undone.`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      confirmColor: 'warn',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.deleteNote();
      }
    });
  }

  private deleteNote(): void {
    if (!this.note.id) {
      this.snackBar.open('Cannot delete note: Invalid note ID', 'Close', {
        duration: 5000,
        panelClass: ['error-snackbar'],
      });
      return;
    }

    this.notesService.clearError();

    this.notesService.deleteNote(this.note.id).subscribe({
      next: (success) => {
        const serviceError = this.notesService.error();
        if (serviceError) {
          this.snackBar.open(serviceError, 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar'],
          });
          return;
        }

        if (success) {
          this.snackBar.open('Note deleted successfully', 'Close', {
            duration: 3000,
          });
          this.noteDeleted.emit(this.note.id);
        } else {
          this.snackBar.open(
            'Failed to delete note. Please try again.',
            'Close',
            {
              duration: 5000,
              panelClass: ['error-snackbar'],
            }
          );
        }
      },
      error: (error) => {
        const errorMessage = extractErrorMessage(error);
        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          panelClass: ['error-snackbar'],
        });
      },
    });
  }

  formatDate(dateString: string | undefined): string {
    if (!dateString) return '';

    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
