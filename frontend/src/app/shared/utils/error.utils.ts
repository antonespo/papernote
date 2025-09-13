import { HttpErrorResponse } from '@angular/common/http';

export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  [key: string]: any;
}

export function extractErrorMessage(error: HttpErrorResponse): string {
  if (error.error && typeof error.error === 'object') {
    const problemDetails = error.error as ProblemDetails;

    if (problemDetails.detail) {
      return problemDetails.detail;
    }

    if (problemDetails.title) {
      return problemDetails.title;
    }
  }

  if (error.message) {
    return error.message;
  }

  switch (error.status) {
    case 400:
      return 'Bad request. Please check your input.';
    case 401:
      return 'Invalid credentials. Please check your username and password.';
    case 403:
      return 'Access forbidden.';
    case 404:
      return 'Resource not found.';
    case 409:
      return 'Conflict. The resource already exists.';
    case 422:
      return 'Validation failed. Please check your input.';
    case 500:
      return 'Internal server error. Please try again later.';
    case 0:
      return 'Network error. Please check your internet connection.';
    default:
      return `An error occurred (${error.status}). Please try again.`;
  }
}
