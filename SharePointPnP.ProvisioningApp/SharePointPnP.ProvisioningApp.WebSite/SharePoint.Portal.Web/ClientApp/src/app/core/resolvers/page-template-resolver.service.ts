import { Injectable } from '@angular/core';
import { NavigationExtras, Resolve, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import { PageTemplate } from '../api/models';
import { PageTemplatesService } from '../api/services';
import { ResolvedData, resolveError } from './resolved-data';

interface PageTemplateResolverOptions {
    /**
     * Provide router navigate arguments if the resolver should navigate on error
     */
    redirectOnError?: { commands: any[], extras?: NavigationExtras };
}

export type ResolvedPageTemplate = ResolvedData<PageTemplate>;

@Injectable({
    providedIn: 'root'
})
export class PageTemplateResolver implements Resolve<ResolvedPageTemplate> {
    constructor(
        private pageTemplatesService: PageTemplatesService,
        private router: Router
    ) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<ResolvedPageTemplate> {
        const options = route.data ?
            route.data.pageTemplateResolverOptions as PageTemplateResolverOptions :
            {};
        const id = route.params.id;

        return this.pageTemplatesService.getPageTemplate(id)
            .pipe(
                map(pageTemplate => ({ data: pageTemplate })),
                catchError(error => this.handleError(options, error.message))
            );
    }

    private handleError(options: PageTemplateResolverOptions, message: string): Observable<ResolvedPageTemplate> {
        if (options.redirectOnError) {
            this.router.navigate(options.redirectOnError.commands, options.redirectOnError.extras);
        }
        return resolveError(message);
    }
}
