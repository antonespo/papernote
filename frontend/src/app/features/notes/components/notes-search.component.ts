import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MatChipEditedEvent, MatChipInputEvent } from '@angular/material/chips';
import { Subject, takeUntil, debounceTime } from 'rxjs';

import { NotesService } from '../services/notes.service';
import { NotesQueryParams } from '../models/notes-search.model';

export interface SearchState {
  searchText: string;
  tags: string[];
  isSearching: boolean;
}

@Component({
  selector: 'app-notes-search',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatAutocompleteModule,
  ],
  template: `
    <div class="search-container">
      <form [formGroup]="searchForm" class="search-form">
        <mat-form-field class="search-field" appearance="outline">
          <mat-label>Cerca nelle note...</mat-label>
          <input
            matInput
            formControlName="searchText"
            placeholder="Inserisci testo da cercare"
            [disabled]="false"
          />
          <mat-icon matSuffix>search</mat-icon>
          @if (isSearching()) {
          <mat-spinner matSuffix diameter="20"></mat-spinner>
          }
        </mat-form-field>

        <mat-form-field class="tags-field" appearance="outline">
          <mat-label>Tags</mat-label>
          <mat-chip-grid #chipGrid aria-label="Selezione tag">
            @for (tag of searchState().tags; track tag) {
            <mat-chip-row
              (removed)="removeTag(tag)"
              [editable]="!isSearching()"
              (edited)="editTag(tag, $event)"
              [aria-description]="'press enter to edit ' + tag"
            >
              {{ tag }}
              <button matChipRemove [attr.aria-label]="'remove ' + tag">
                <mat-icon>cancel</mat-icon>
              </button>
            </mat-chip-row>
            }
            <input
              placeholder="Aggiungi tag..."
              [matChipInputFor]="chipGrid"
              [matChipInputSeparatorKeyCodes]="separatorKeysCodes"
              [matChipInputAddOnBlur]="true"
              (matChipInputTokenEnd)="addTag($event)"
              [disabled]="isSearching()"
            />
          </mat-chip-grid>
        </mat-form-field>

        <button
          mat-icon-button
          type="button"
          (click)="clearSearch()"
          matTooltip="Cancella ricerca"
          [disabled]="
            isSearching() ||
            (!searchState().searchText && searchState().tags.length === 0)
          "
        >
          <mat-icon>clear</mat-icon>
        </button>
      </form>
    </div>
  `,
  styles: [
    `
      .search-container {
        padding: 16px;
        background: var(--mat-app-surface);
        border-bottom: 1px solid var(--mat-app-outline-variant);
      }

      .search-form {
        display: flex;
        gap: 16px;
        align-items: flex-start;
        max-width: 1200px;
        margin: 0 auto;
      }

      .search-field {
        flex: 2;
        min-width: 300px;
      }

      .tags-field {
        flex: 3;
        min-width: 400px;
      }

      @media (max-width: 768px) {
        .search-form {
          flex-direction: column;
          gap: 12px;
        }

        .search-field,
        .tags-field {
          flex: 1;
          min-width: unset;
          width: 100%;
        }
      }

      .mat-mdc-form-field-subscript-wrapper {
        display: none;
      }
    `,
  ],
})
export class NotesSearchComponent implements OnInit, OnDestroy {
  private readonly formBuilder = inject(FormBuilder);
  private readonly notesService = inject(NotesService);
  private readonly destroy$ = new Subject<void>();

  readonly separatorKeysCodes = [ENTER, COMMA] as const;
  readonly searchState = signal<SearchState>({
    searchText: '',
    tags: [],
    isSearching: false,
  });

  readonly isSearching = this.notesService.isLoading;

  searchForm: FormGroup;
  currentFilter: 'owned' | 'shared' = 'owned';

  constructor() {
    this.searchForm = this.formBuilder.group({
      searchText: [''],
    });
  }

  ngOnInit(): void {
    this.setupSearchDebounce();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  setFilter(filter: 'owned' | 'shared'): void {
    this.currentFilter = filter;
    this.triggerSearch();
  }

  private setupSearchDebounce(): void {
    this.searchForm
      .get('searchText')
      ?.valueChanges.pipe(debounceTime(300), takeUntil(this.destroy$))
      .subscribe((searchText: string) => {
        this.searchState.update((state) => ({
          ...state,
          searchText: searchText || '',
        }));
        this.triggerSearch();
      });
  }

  addTag(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value && !this.searchState().tags.includes(value)) {
      this.searchState.update((state) => ({
        ...state,
        tags: [...state.tags, value],
      }));
      this.triggerSearch();
    }
    event.chipInput!.clear();
  }

  removeTag(tag: string): void {
    this.searchState.update((state) => ({
      ...state,
      tags: state.tags.filter((t) => t !== tag),
    }));
    this.triggerSearch();
  }

  editTag(oldTag: string, event: MatChipEditedEvent): void {
    const newTag = event.value.trim();
    if (!newTag) {
      this.removeTag(oldTag);
      return;
    }

    if (newTag !== oldTag && !this.searchState().tags.includes(newTag)) {
      this.searchState.update((state) => ({
        ...state,
        tags: state.tags.map((tag) => (tag === oldTag ? newTag : tag)),
      }));
      this.triggerSearch();
    }
  }

  clearSearch(): void {
    this.searchForm.get('searchText')?.setValue('');
    this.searchState.update((state) => ({
      ...state,
      searchText: '',
      tags: [],
    }));
    this.triggerSearch();
  }

  private triggerSearch(): void {
    const state = this.searchState();
    const hasSearchCriteria = state.searchText.trim() || state.tags.length > 0;

    if (hasSearchCriteria) {
      const queryParams: NotesQueryParams = {
        searchText: state.searchText.trim(),
        selectedTags: state.tags,
        filterType: this.currentFilter === 'owned' ? 'own' : 'shared',
      };
      this.notesService.searchNotes(queryParams);
    } else {
      this.notesService.loadNotes(this.currentFilter).subscribe();
    }
  }
}
