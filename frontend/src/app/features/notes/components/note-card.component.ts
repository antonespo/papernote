import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NoteSummaryDto } from '../../../api/notes';

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
  @Input({ required: true }) note!: NoteSummaryDto;
  @Input() isOwned: boolean = true;

  onNoteClick(): void {
    // TODO: Navigate to note detail
    console.log('Navigate to note:', this.note.id);
  }

  onEditClick(event: Event): void {
    event.stopPropagation();
    // TODO: Navigate to note edit
    console.log('Edit note:', this.note.id);
  }

  onDeleteClick(event: Event): void {
    event.stopPropagation();
    // TODO: Delete note
    console.log('Delete note:', this.note.id);
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
