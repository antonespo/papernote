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
  templateUrl: './notes-search.component.html',
  styleUrl: './notes-search.component.scss',
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
