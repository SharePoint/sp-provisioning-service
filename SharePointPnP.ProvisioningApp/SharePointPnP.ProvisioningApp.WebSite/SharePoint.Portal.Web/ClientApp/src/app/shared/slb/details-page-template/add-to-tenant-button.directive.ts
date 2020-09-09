import { Directive, HostListener, OnInit, OnDestroy } from '@angular/core';
import { takeUntil, filter } from 'rxjs/operators';
import { Subject, combineLatest } from 'rxjs';

import { DetailsPageTemplateDataService } from './details-page-template-data.service';
import { ActivatedRoute } from '@angular/router';

@Directive({
    // tslint:disable-next-line: directive-selector
    selector: '[slbAddToTenantButton]'
})
export class AddToTenantButtonDirective implements OnInit, OnDestroy {
    get provisioningFormUrl(): string {
        return this.url;
    }

    private url: string;
    private isInitialized: boolean;
    private destroy = new Subject<void>();

    constructor(
        private dataService: DetailsPageTemplateDataService,
        private route: ActivatedRoute
    ) { }

    ngOnInit() {
        combineLatest(this.dataService.data
            .pipe(
                takeUntil(this.destroy),
                filter(packageData => !!packageData.provisioningFormUrl),
            ),
            this.route.queryParams)
            .subscribe(([packageData, queryParams]) => {
                const currentLocation = encodeURIComponent(location.href);

                let queryString = '';
                // Add source, if available
                if (queryParams.source) {
                    queryString = `source=${queryParams.source}&`;
                }
                queryString += `returnUrl=${currentLocation}`;

                this.url = packageData.provisioningFormUrl;
                // Add return URL to the form url
                if (packageData.provisioningFormUrl.includes('?')) {
                    this.url += '&';
                } else {
                    this.url += '?';
                }
                this.url += queryString;
                this.isInitialized = true;
            });
    }

    ngOnDestroy() {
        this.destroy.next();
        this.destroy.complete();
    }

    @HostListener('click') openProvisioningTab() {
        if (this.isInitialized) {
            location.href = this.provisioningFormUrl;
        }
    }
}
