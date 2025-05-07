import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideToastr } from 'ngx-toastr';

export const appConfig: ApplicationConfig = {
  providers: [provideZoneChangeDetection({ eventCoalescing: true }),provideClientHydration(withEventReplay()), provideRouter(routes),provideHttpClient(withFetch()),provideAnimations(),
    provideToastr({
      positionClass : 'toast-bottom-right',
      timeOut : 3000,
      preventDuplicates : true
    })]
};
