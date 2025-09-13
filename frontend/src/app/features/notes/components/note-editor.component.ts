import {
  Component,
  inject,
  signal,
  computed,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormControl,
  Validators,
  FormArray,
  AbstractControl,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MatChipEditedEvent, MatChipInputEvent } from '@angular/material/chips';
import { Subject, takeUntil, debounceTime } from 'rxjs';

import { NotesService } from '../services/notes.service';
import { NoteFormData, NoteFormState } from '../models/note-form.model';
import { NoteDto, CreateNoteDto, UpdateNoteDto } from '../../../api/notes';
import { extractErrorMessage } from '../../../shared/utils/error.utils';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog.component';

interface NoteForm {
  title: FormControl<string>;
  content: FormControl<string>;
  tags: FormArray<FormControl<string>>;
  sharedWith: FormArray<FormControl<string>>;
}

@Component({
  selector: 'app-note-editor',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
  ],
  templateUrl: './note-editor.component.html',
  styleUrl: './note-editor.component.scss',
})
export class NoteEditorComponent implements OnInit, OnDestroy {
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notesService = inject(NotesService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();
  private originalFormData: { tags: string[]; sharedWith: string[] } = {
    tags: [],
    sharedWith: [],
  };

  readonly separatorKeysCodes = [ENTER, COMMA] as const;
  readonly isEditMode = signal(false);
  readonly isReadOnly = signal(false);
  readonly noteId = signal<string | null>(null);
  readonly formState = signal<NoteFormState>({
    isLoading: false,
    isSaving: false,
    error: null,
    isDirty: false,
    isValid: false,
  });

  readonly serviceError = this.notesService.error;

  noteForm: FormGroup<NoteForm>;

  readonly tagsArray = computed(
    () => this.noteForm.get('tags') as FormArray<FormControl<string>>
  );
  readonly sharedWithArray = computed(
    () => this.noteForm.get('sharedWith') as FormArray<FormControl<string>>
  );

  readonly canSave = computed(() => {
    const state = this.formState();
    return (
      !state.isSaving && state.isDirty && state.isValid && !this.isReadOnly()
    );
  });

  constructor() {
    this.noteForm = this.formBuilder.group<NoteForm>({
      title: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(200),
        ],
      }),
      content: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(10)],
      }),
      tags: this.formBuilder.array<FormControl<string>>([], {
        validators: [this.maxArrayLength(10)],
      }),
      sharedWith: this.formBuilder.array<FormControl<string>>([], {
        validators: [this.maxArrayLength(20)],
      }),
    });
  }

  ngOnInit(): void {
    this.route.params.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const id = params['id'];
      if (id && id !== 'new') {
        this.noteId.set(id);
        this.isEditMode.set(true);
        this.loadNote(id);
      }
    });

    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe((queryParams) => {
        this.isReadOnly.set(queryParams['readonly'] === 'true');
        this.setupFormForMode();
      });

    this.noteForm.valueChanges
      .pipe(debounceTime(300), takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateFormState();
      });

    this.updateFormState();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private maxArrayLength(max: number) {
    return (control: AbstractControl) => {
      if (control instanceof FormArray) {
        return control.length > max
          ? { maxlength: { max, actual: control.length } }
          : null;
      }
      return null;
    };
  }

  private updateFormState(): void {
    const isFormDirty = this.noteForm.dirty || this.hasArraysChanged();

    this.formState.update((state) => ({
      ...state,
      isDirty: this.isReadOnly() ? false : isFormDirty,
      isValid: this.isReadOnly() ? true : this.noteForm.valid,
    }));
  }

  private hasArraysChanged(): boolean {
    const currentTags = this.tagsArray().value;
    const currentShared = this.sharedWithArray().value;

    return (
      !this.arraysEqual(currentTags, this.originalFormData.tags) ||
      !this.arraysEqual(currentShared, this.originalFormData.sharedWith)
    );
  }

  private arraysEqual(a: string[], b: string[]): boolean {
    if (a.length !== b.length) return false;
    const sortedA = [...a].sort();
    const sortedB = [...b].sort();
    return sortedA.every((val, index) => val === sortedB[index]);
  }

  private setupFormForMode(): void {
    if (this.isReadOnly()) {
      this.noteForm.disable();
    } else {
      this.noteForm.enable();
    }
    this.updateFormState();
  }

  private loadNote(id: string): void {
    this.formState.update((state) => ({
      ...state,
      isLoading: true,
      error: null,
    }));

    this.notesService.loadNote(id).subscribe({
      next: (note: NoteDto | null) => {
        if (note) {
          this.populateForm(note);
        } else {
          this.setError('Note not found');
        }
      },
      error: (error) => {
        this.setError(extractErrorMessage(error));
      },
      complete: () => {
        this.formState.update((state) => ({ ...state, isLoading: false }));
      },
    });
  }

  private populateForm(note: NoteDto): void {
    this.noteForm.patchValue({
      title: note.title || '',
      content: note.content || '',
    });

    const tags = note.tags || [];
    const sharedWith = note.sharedWithUsernames || [];

    this.setTags(tags);
    this.setSharedWith(sharedWith);

    this.originalFormData = {
      tags: [...tags],
      sharedWith: [...sharedWith],
    };

    this.noteForm.markAsPristine();
    this.setupFormForMode();
  }

  private setTags(tags: string[]): void {
    const tagsArray = this.tagsArray();
    tagsArray.clear();
    tags.forEach((tag) => {
      tagsArray.push(new FormControl(tag, { nonNullable: true }));
    });
  }

  private setSharedWith(usernames: string[]): void {
    const sharedArray = this.sharedWithArray();
    sharedArray.clear();
    usernames.forEach((username) => {
      sharedArray.push(new FormControl(username, { nonNullable: true }));
    });
  }

  addTag(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value && !this.tagsArray().value.includes(value)) {
      this.tagsArray().push(new FormControl(value, { nonNullable: true }));
      this.updateFormState();
    }
    event.chipInput!.clear();
  }

  removeTag(index: number): void {
    this.tagsArray().removeAt(index);
    this.updateFormState();
  }

  editTag(index: number, event: MatChipEditedEvent): void {
    const value = event.value.trim();
    if (!value) {
      this.removeTag(index);
      return;
    }

    if (
      !this.tagsArray().value.includes(value) ||
      this.tagsArray().at(index).value === value
    ) {
      this.tagsArray().at(index).setValue(value);
      this.updateFormState();
    }
  }

  addSharedUser(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value && !this.sharedWithArray().value.includes(value)) {
      this.sharedWithArray().push(
        new FormControl(value, { nonNullable: true })
      );
      this.updateFormState();
    }
    event.chipInput!.clear();
  }

  removeSharedUser(index: number): void {
    this.sharedWithArray().removeAt(index);
    this.updateFormState();
  }

  editSharedUser(index: number, event: MatChipEditedEvent): void {
    const value = event.value.trim();
    if (!value) {
      this.removeSharedUser(index);
      return;
    }

    if (
      !this.sharedWithArray().value.includes(value) ||
      this.sharedWithArray().at(index).value === value
    ) {
      this.sharedWithArray().at(index).setValue(value);
      this.updateFormState();
    }
  }

  onSave(): void {
    if (!this.canSave()) return;

    this.notesService.clearError();

    this.formState.update((state) => ({
      ...state,
      isSaving: true,
      error: null,
    }));

    const formData: NoteFormData = {
      title: this.noteForm.value.title!,
      content: this.noteForm.value.content!,
      tags: this.tagsArray().value,
      sharedWith: this.sharedWithArray().value,
    };

    const saveOperation = this.isEditMode()
      ? this.updateNote(this.noteId()!, formData)
      : this.createNote(formData);

    saveOperation.subscribe({
      next: (result) => {
        const serviceError = this.serviceError();
        if (serviceError) {
          this.setError(serviceError);
          return;
        }

        if (result) {
          this.snackBar.open(
            this.isEditMode()
              ? 'Note updated successfully'
              : 'Note created successfully',
            'Close',
            { duration: 3000 }
          );
          this.router.navigate(['/notes']);
        } else {
          this.setError(
            'Failed to save note. Please check your data and try again.'
          );
        }
      },
      error: (error) => {
        this.setError(extractErrorMessage(error));
      },
      complete: () => {
        this.formState.update((state) => ({ ...state, isSaving: false }));
      },
    });
  }

  private createNote(formData: NoteFormData) {
    const createDto: CreateNoteDto = {
      title: formData.title,
      content: formData.content,
      tags: formData.tags,
      sharedWithUsernames: formData.sharedWith,
    };
    return this.notesService.createNote(createDto);
  }

  private updateNote(id: string, formData: NoteFormData) {
    const updateDto: UpdateNoteDto = {
      id: id,
      title: formData.title,
      content: formData.content,
      tags: formData.tags,
      sharedWithUsernames: formData.sharedWith,
    };
    return this.notesService.updateNote(id, updateDto);
  }

  onDelete(): void {
    const dialogData: ConfirmDialogData = {
      title: 'Delete Note',
      message:
        'Are you sure you want to delete this note? This action cannot be undone.',
      confirmText: 'Delete',
      cancelText: 'Cancel',
      confirmColor: 'warn',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && this.noteId()) {
        this.deleteNote(this.noteId()!);
      }
    });
  }

  private deleteNote(id: string): void {
    this.notesService.clearError();

    this.formState.update((state) => ({
      ...state,
      isSaving: true,
      error: null,
    }));

    this.notesService.deleteNote(id).subscribe({
      next: (success) => {
        const serviceError = this.serviceError();
        if (serviceError) {
          this.setError(serviceError);
          return;
        }

        if (success) {
          this.snackBar.open('Note deleted successfully', 'Close', {
            duration: 3000,
          });
          this.router.navigate(['/notes']);
        } else {
          this.setError('Failed to delete note. Please try again.');
        }
      },
      error: (error) => {
        this.setError(extractErrorMessage(error));
      },
      complete: () => {
        this.formState.update((state) => ({ ...state, isSaving: false }));
      },
    });
  }

  onCancel(): void {
    if (this.formState().isDirty) {
      const dialogData: ConfirmDialogData = {
        title: 'Unsaved Changes',
        message:
          'You have unsaved changes. Are you sure you want to leave without saving?',
        confirmText: 'Leave',
        cancelText: 'Stay',
        confirmColor: 'warn',
      };

      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '400px',
        data: dialogData,
      });

      dialogRef.afterClosed().subscribe((result) => {
        if (result) {
          this.router.navigate(['/notes']);
        }
      });
    } else {
      this.router.navigate(['/notes']);
    }
  }

  private setError(message: string): void {
    this.formState.update((state) => ({ ...state, error: message }));
  }
}
