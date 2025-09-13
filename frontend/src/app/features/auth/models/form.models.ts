import { FormControl } from '@angular/forms';

export interface LoginForm {
  username: FormControl<string>;
  password: FormControl<string>;
}

export interface RegisterForm {
  username: FormControl<string>;
  password: FormControl<string>;
  confirmPassword: FormControl<string>;
}
