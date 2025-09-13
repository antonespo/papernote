import { UserDto } from '../../api/auth';
import { NoteDto, NoteSummaryDto } from '../../api/notes';

export interface LoadingState {
  readonly isLoading: boolean;
  readonly error: string | null;
}

export interface AuthState extends LoadingState {
  readonly isAuthenticated: boolean;
  readonly user: UserDto | null;
  readonly accessToken: string | null;
  readonly refreshToken: string | null;
  readonly expiresAt: Date | null;
}

export interface NotesState extends LoadingState {
  readonly notes: readonly NoteSummaryDto[];
  readonly selectedNote: NoteDto | null;
}
