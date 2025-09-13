import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideApi as provideAuthApi } from './api/auth/provide-api';
import { provideApi as provideNotesApi } from './api/notes/provide-api';
import { environment } from '../environments/environment';
import { jwtInterceptor } from './shared/interceptors/jwt-functional.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([jwtInterceptor])),
    provideAuthApi(environment.apiConfig.auth),
    provideNotesApi(environment.apiConfig.notes),
  ],
};
