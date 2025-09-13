import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, of, EMPTY, Subject } from 'rxjs';
import {
  map,
  catchError,
  tap,
  finalize,
  debounceTime,
  switchMap,
  distinctUntilChanged,
} from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { NotesState } from '../../../shared/models/state.model';
import {
  NotesSearchParams,
  NotesQueryParams,
} from '../models/notes-search.model';
import {
  NoteDto,
  NoteSummaryDto,
  CreateNoteDto,
  UpdateNoteDto,
  NotesService as NotesApiService,
} from '../../../api/notes';
import { extractErrorMessage } from '../../../shared/utils/error.utils';

@Injectable({
  providedIn: 'root',
})
export class NotesService {
  private readonly notesApi = inject(NotesApiService);
  private readonly searchSubject = new Subject<NotesQueryParams>();

  private readonly notesState = signal<NotesState>({
    isLoading: false,
    error: null,
    notes: [],
    selectedNote: null,
  });

  readonly isLoading = computed(() => this.notesState().isLoading);
  readonly error = computed(() => this.notesState().error);
  readonly notes = computed(() => this.notesState().notes);
  readonly selectedNote = computed(() => this.notesState().selectedNote);

  constructor() {
    this.setupSearchStream();
  }

  private setupSearchStream(): void {
    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(
          (prev, curr) =>
            prev.searchText === curr.searchText &&
            prev.filterType === curr.filterType &&
            JSON.stringify(prev.selectedTags) ===
              JSON.stringify(curr.selectedTags)
        ),
        switchMap((queryParams) => this.loadNotesInternal(queryParams))
      )
      .subscribe();
  }

  loadNotes(queryParams?: NotesQueryParams): Observable<NoteSummaryDto[]> {
    return this.loadNotesInternal(queryParams);
  }

  searchNotes(queryParams: NotesQueryParams): void {
    this.searchSubject.next(queryParams);
  }

  private loadNotesInternal(
    queryParams?: NotesQueryParams
  ): Observable<NoteSummaryDto[]> {
    this.setLoading(true);

    const searchParams = this.buildSearchParams(queryParams);

    return this.notesApi
      .getNotes(searchParams.filter, searchParams.text, searchParams.tags)
      .pipe(
        tap((notes: NoteSummaryDto[]) => {
          this.notesState.update((state) => ({
            ...state,
            notes,
            error: null,
          }));
        }),
        catchError((error: HttpErrorResponse) => {
          const errorMessage = extractErrorMessage(error);
          this.setError(errorMessage);
          return of([]);
        }),
        finalize(() => {
          this.setLoading(false);
        })
      );
  }

  private buildSearchParams(queryParams?: NotesQueryParams): NotesSearchParams {
    if (!queryParams) {
      return {};
    }

    const searchParams: NotesSearchParams = {};

    if (queryParams.filterType) {
      searchParams.filter = queryParams.filterType;
    }

    if (queryParams.searchText) {
      searchParams.text = queryParams.searchText;
    }

    if (queryParams.selectedTags && queryParams.selectedTags.length > 0) {
      searchParams.tags = queryParams.selectedTags.join(',');
    }

    return searchParams;
  }

  loadNote(id: string): Observable<NoteDto | null> {
    this.setLoading(true);

    return this.notesApi.getNoteById(id).pipe(
      tap((note: NoteDto) => {
        this.notesState.update((state) => ({
          ...state,
          selectedNote: note,
          error: null,
        }));
      }),
      catchError((error: HttpErrorResponse) => {
        const errorMessage = extractErrorMessage(error);
        this.setError(errorMessage);
        return of(null);
      }),
      finalize(() => {
        this.setLoading(false);
      })
    );
  }

  createNote(createNoteDto: CreateNoteDto): Observable<NoteDto | null> {
    this.setLoading(true);

    return this.notesApi.createNote(createNoteDto).pipe(
      tap((newNote: NoteDto) => {
        const notePreview: NoteSummaryDto = {
          id: newNote.id,
          title: newNote.title,
          contentPreview: newNote.content?.substring(0, 100) + '...',
          tags: newNote.tags,
          createdAt: newNote.createdAt,
          updatedAt: newNote.updatedAt,
          ownerUsername: newNote.ownerUsername,
        };

        this.notesState.update((state) => ({
          ...state,
          notes: [notePreview, ...state.notes],
          error: null,
        }));
      }),
      catchError((error: HttpErrorResponse) => {
        const errorMessage = extractErrorMessage(error);
        this.setError(errorMessage);
        return of(null);
      }),
      finalize(() => {
        this.setLoading(false);
      })
    );
  }

  updateNote(
    id: string,
    updateNoteDto: UpdateNoteDto
  ): Observable<NoteDto | null> {
    this.setLoading(true);

    return this.notesApi.updateNote(id, updateNoteDto).pipe(
      tap((updatedNote: NoteDto) => {
        this.notesState.update((state) => ({
          ...state,
          notes: state.notes.map((note) =>
            note.id === id
              ? {
                  ...note,
                  title: updatedNote.title || note.title,
                  contentPreview:
                    updatedNote.content?.substring(0, 100) + '...' ||
                    note.contentPreview,
                  tags: updatedNote.tags || note.tags,
                  updatedAt: updatedNote.updatedAt || new Date().toISOString(),
                }
              : note
          ),
          selectedNote:
            state.selectedNote?.id === id ? updatedNote : state.selectedNote,
          error: null,
        }));
      }),
      catchError((error: HttpErrorResponse) => {
        const errorMessage = extractErrorMessage(error);
        this.setError(errorMessage);
        return of(null);
      }),
      finalize(() => {
        this.setLoading(false);
      })
    );
  }

  deleteNote(id: string): Observable<boolean> {
    this.setLoading(true);

    return this.notesApi.deleteNote(id).pipe(
      tap(() => {
        this.notesState.update((state) => ({
          ...state,
          notes: state.notes.filter((note) => note.id !== id),
          selectedNote:
            state.selectedNote?.id === id ? null : state.selectedNote,
          error: null,
        }));
      }),
      map(() => true),
      catchError((error: HttpErrorResponse) => {
        const errorMessage = extractErrorMessage(error);
        this.setError(errorMessage);
        return of(false);
      }),
      finalize(() => {
        this.setLoading(false);
      })
    );
  }

  clearSelectedNote(): void {
    this.notesState.update((state) => ({
      ...state,
      selectedNote: null,
    }));
  }

  private setLoading(isLoading: boolean): void {
    this.notesState.update((state) => ({ ...state, isLoading }));
  }

  private setError(error: string): void {
    this.notesState.update((state) => ({ ...state, error }));
  }

  clearError(): void {
    this.notesState.update((state) => ({ ...state, error: null }));
  }
}
