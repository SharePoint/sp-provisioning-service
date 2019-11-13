import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot, NavigationExtras } from '@angular/router';
import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import { Package } from '../api/models';
import { PackagesService } from '../api/services';
import { guidRegex } from '../api/utilities';
import { ResolvedData, resolveError } from './resolved-data';

interface PackageResolverOptions {
    /**
     * Provide router navigate arguments if the resolver should navigate on error
     */
    redirectOnError?: { commands: any[], extras?: NavigationExtras };
}

export type ResolvedPackage = ResolvedData<Package>;

@Injectable({
    providedIn: 'root'
})
export class PackageResolver implements Resolve<ResolvedPackage> {
    constructor(
        private packagesService: PackagesService,
        private router: Router
    ) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<ResolvedPackage> {
        const options: PackageResolverOptions = route.data && route.data.packageResolverOptions ?
            route.data.packageResolverOptions : {};
        const id = route.params.id;

        if (!guidRegex.test(id)) {
            return this.handleError(options, 'Invalid id provided');
        }

        return this.packagesService.getPackageById(id)
            .pipe(
                map(packageData => ({ data: packageData })),
                catchError(error => this.handleError(options, error.message))
            );
    }

    private handleError(options, message): Observable<ResolvedPackage> {
        if (options.redirectOnError) {
            this.router.navigate(options.redirectOnError.commands, options.redirectOnError.extras);
        }
        return resolveError(message);
    }
}
