import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class NoteValidators {
  static uniqueArrayValues(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value || !Array.isArray(control.value)) {
        return null;
      }

      const values = control.value;
      const uniqueValues = [...new Set(values)];

      return values.length !== uniqueValues.length
        ? { duplicateValues: true }
        : null;
    };
  }

  static maxArrayLength(max: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value || !Array.isArray(control.value)) {
        return null;
      }

      return control.value.length > max
        ? { maxArrayLength: { max, actual: control.value.length } }
        : null;
    };
  }

  static validUsername(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const usernamePattern = /^[a-zA-Z0-9_]{3,20}$/;
      return usernamePattern.test(control.value)
        ? null
        : { invalidUsername: true };
    };
  }

  static validTag(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const tagPattern = /^[a-zA-Z0-9_-]{1,30}$/;
      return tagPattern.test(control.value) ? null : { invalidTag: true };
    };
  }
}
