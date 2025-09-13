import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';

import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideApi as provideAuthApi } from './api/auth/provide-api';
import { provideApi as provideNotesApi } from './api/notes/provide-api';
import { environment } from '../environments/environment';
// import { jwtInterceptor } from './core/interceptors/jwt.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(),
    provideAuthApi(environment.apiConfig.auth),
    provideNotesApi(environment.apiConfig.notes),
    // provideHttpClient(withInterceptors([jwtInterceptor]))
  ],
};
