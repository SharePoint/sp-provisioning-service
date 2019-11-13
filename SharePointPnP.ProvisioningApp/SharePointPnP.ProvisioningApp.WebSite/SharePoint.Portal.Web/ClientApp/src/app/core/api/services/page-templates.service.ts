import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

import { PageTemplate } from '../models';
import { toHttpParams } from '../utilities';

@Injectable({
    providedIn: 'root'
})
export class PageTemplatesService {
    private cachedTemplates: { [key: string]: Observable<PageTemplate> } = {};
    private readonly url;

    constructor(
        private http: HttpClient
    ) {
        this.url = '/api/pagetemplates';
    }

    getPageTemplate(id: string): Observable<PageTemplate> {
        let cachedResponse: Observable<PageTemplate> = this.cachedTemplates && this.cachedTemplates[id];
        if (!cachedResponse) {
            const params = toHttpParams({ templateId: id });
            cachedResponse = this.http.get<PageTemplate>(this.url, { params })
                .pipe(shareReplay(1));
            this.cachedTemplates[id] = cachedResponse;
        }
        return cachedResponse;
    }
}
