import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot, Router, NavigationExtras } from '@angular/router';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { CategoriesService } from '../api/services';
import { Category } from '../api/models';
import { ResolvedData, resolveError } from './resolved-data';

interface CategoriesResolverOptions {
    /**
     * Provide router navigate arguments if the resolver should navigate on error
     */
    redirectOnError?: { commands: any[], extras?: NavigationExtras};
}

export type ResolvedCategories = ResolvedData<Category[]>;

@Injectable({
    providedIn: 'root'
})
export class CategoriesResolver implements Resolve<ResolvedCategories> {
    constructor(
        private categoryService: CategoriesService,
        private router: Router
    ) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<ResolvedCategories> {
        return this.categoryService.getAll()
            .pipe(
                map(categories => ({data: categories})),
                catchError(error => {
                    if (route.data && route.data.categoriesResolverOptions) {
                        const options = route.data.categoriesResolverOptions as CategoriesResolverOptions;
                        if (options.redirectOnError) {
                            this.router.navigate(options.redirectOnError.commands, options.redirectOnError.extras);
                        }
                    }
                    return resolveError(error.message);
                })
            );
    }
}
