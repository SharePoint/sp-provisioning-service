import { Directive, HostListener, OnInit, OnDestroy } from '@angular/core';
import { takeUntil, filter } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { DetailsPageTemplateDataService } from './details-page-template-data.service';

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
    ) { }

    ngOnInit() {
        this.dataService.data
            .pipe(
                takeUntil(this.destroy),
                filter(packageData => !!packageData.provisioningFormUrl)
            )
            .subscribe(packageData => {
                const currentLocation = `${location.protocol}//${location.host}${location.pathname}`;

                // Add return URL to the form url
                const queryString = packageData.provisioningFormUrl.includes('?') ?
                    `&returnUrl=${currentLocation}` : `?returnUrl=${currentLocation}`;

                this.url = `${packageData.provisioningFormUrl}${queryString}`;
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
