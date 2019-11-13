import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

import { toHttpParams } from '../utilities';
import { Category } from '../models/category.model';

export class CategoriesQueryParams {
    doIncludeDisplayInfo: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class CategoriesService {
    url: string;

    private cachedCategories: Observable<Category[]>;

    constructor(private http: HttpClient) {
        this.url = '/api/categories';
    }

    /**
     * Makes http call to get the categories and the template cards.
     * Keeps cached results while the page is open.
     */
    getAll(filter?: CategoriesQueryParams): Observable<Category[]> {
        // TODO: In the future we may want to lazy load the extra package details
        filter = filter || new CategoriesQueryParams();
        filter.doIncludeDisplayInfo = true;

        const params = toHttpParams(filter);
        if (!this.cachedCategories) {
            this.cachedCategories = this.http.get<Category[]>(this.url, { params })
                .pipe(
                    shareReplay()
                );
        }
        return this.cachedCategories;
    }
}
