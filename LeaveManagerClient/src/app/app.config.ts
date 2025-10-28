    import { ApplicationConfig } from '@angular/core';
    import { provideHttpClient } from '@angular/common/http';
    import { HttpClient } from '@angular/common/http';

    export const appConfig: ApplicationConfig = {
      providers: [
        provideHttpClient(),
        HttpClient
      ],
    };