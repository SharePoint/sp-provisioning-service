import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Package } from '../models';

@Injectable({
    providedIn: 'root'
})
export class PackagesService {
    url: string;

    constructor(private http: HttpClient) {
        this.url = '/api/packages';
    }

    getPackages(): any {
        return this.http.get<any>(this.url);
    }

    getPackageById(id: string): Observable<Package> {
        return this.http.get<Package>(`${this.url}/${id}`);
    }
}
