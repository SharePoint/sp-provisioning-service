import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Tracking } from '../models';
import { ActivatedRoute } from '@angular/router';
import { empty, zip, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ApplicationSettingsService } from '.';

@Injectable({
    providedIn: 'root'
})
export class TrackingService {
    constructor(
        private http: HttpClient,
        private appSettings: ApplicationSettingsService) {
    }

    track(route: ActivatedRoute, tracking: Partial<Tracking> = {}) {
        combineLatest(route.queryParams, this.appSettings.getSettings())
            .pipe(
                switchMap(([queryParams, settings]) => {
                    let source = queryParams.source;
                    // Nothing to do if refererId is not available
                    if (!source || !settings.trackingUrl) {
                        source = 'default';
                    }
                    const req: Tracking = {
                        SourceId: source,
                        SourceTrackingAction: '0',
                        SourceTrackingFromProduction: typeof settings.isTestEnvironment === 'undefined' ? 'false' : (settings.isTestEnvironment ? 'false' : 'true'),
                        SourceTrackingUrl: document.location.origin + document.location.pathname
                    };
                    // Overwrite tracking fields
                    if (tracking) {
                        Object.assign(req, tracking);
                    }
                    
                    return this.http.post(settings.trackingUrl, req);
                })
            ).subscribe();
    }
}
