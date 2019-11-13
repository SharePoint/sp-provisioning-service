import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

import { ApplicationSettings } from '../models/application-settings.model';

@Injectable({
    providedIn: 'root'
})
export class ApplicationSettingsService {
    private readonly url: string;
    private cachedSettings: Observable<ApplicationSettings>;

    constructor(
        private http: HttpClient
    ) {
        this.url = '/api/settings';
    }

    /**
     * Makes http call to get the application settings from the server
     */
    getSettings(): Observable<ApplicationSettings> {
        if (!this.cachedSettings) {
            this.cachedSettings = this.http.get<ApplicationSettings>(this.url)
                .pipe(shareReplay(1));
        }
        return this.cachedSettings;
    }
}
