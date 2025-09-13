export interface NoteFormData {
  title: string;
  content: string;
  tags: string[];
  sharedWith: string[];
}

export interface NoteFormValidation {
  title: {
    required: boolean;
    minLength: boolean;
    maxLength: boolean;
  };
  content: {
    required: boolean;
    minLength: boolean;
  };
  tags: {
    maxCount: boolean;
    invalidFormat: boolean;
  };
  sharedWith: {
    maxCount: boolean;
    invalidFormat: boolean;
  };
}

export interface NoteFormState {
  isLoading: boolean;
  isSaving: boolean;
  error: string | null;
  isDirty: boolean;
  isValid: boolean;
}
