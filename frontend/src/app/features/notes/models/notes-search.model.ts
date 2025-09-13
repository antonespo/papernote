export interface NotesSearchParams {
  filter?: string;
  text?: string;
  tags?: string;
}

export interface NotesQueryParams {
  readonly searchText?: string;
  readonly selectedTags?: readonly string[];
  readonly filterType?: 'all' | 'shared' | 'own';
}
