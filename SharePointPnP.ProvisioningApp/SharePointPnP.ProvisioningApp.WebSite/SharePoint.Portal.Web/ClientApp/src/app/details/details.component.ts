import { Component, OnInit, OnDestroy, NgModule, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material';
import { Title } from '@angular/platform-browser';
import { Subject, EMPTY } from 'rxjs';
import { switchMap, catchError, map, tap, distinctUntilChanged } from 'rxjs/operators';

import { appConstants } from '../app-constants';
import { PageTemplatesService, ApplicationSettingsService, TrackingService } from '../core/api/services';
import { ResolvedPackage } from '../core/resolvers/package-resolver.service';
import { DetailsPageTemplateData } from '../shared/slb/details-page-template/details-page-template-data.service';

import { SlbButtonModule } from '../shared/slb/button/button.module';
import { SlbCarouselModule } from '../shared/slb/carousel/carousel.module';
import { SlbExpandablePanelModule } from '../shared/slb/expandable-panel/expandable-panel.module';
import { SlbStickyHeaderModule } from '../shared/slb/sticky-header/sticky-header.module';
import { SlbTabsModule } from '../shared/slb/tabs/tabs.module';
import { SlbDetailsPageTemplateModule } from '../shared/slb/details-page-template/details-page-template.module';

import { ApplicationSettings } from '../core/api/models';
import { environment } from 'src/environments/environment';

interface DetailsTemplateDefinition {
    moduleDefinition: NgModule;
    html: string;
    css: string;
    context: DetailsPageTemplateData;
}

const detailsModuleDefinition = {
    imports: [
        CommonModule,
        SlbButtonModule,
        SlbCarouselModule,
        SlbExpandablePanelModule,
        SlbStickyHeaderModule,
        SlbTabsModule,
        SlbDetailsPageTemplateModule,
    ]
};

@Component({
    templateUrl: './details.component.html',
    styleUrls: ['./details.component.scss'],
})
export class DetailsComponent implements OnInit, OnDestroy {
    detailsTemplateDefinition: DetailsTemplateDefinition;

    doDisplayDefaultTemplate: boolean;

    private componentDestroy = new Subject<void>();
    private pageData: DetailsPageTemplateData;

    constructor(
        private route: ActivatedRoute,
        private trackingService: TrackingService,
        private snackBar: MatSnackBar,
        private appSettings: ApplicationSettingsService,
        private titleService: Title,
        private pageTemplateService: PageTemplatesService,
        private changeDetectorRef: ChangeDetectorRef,
    ) { }

    ngOnInit() {
        this.route.params
            .pipe(
                tap(() => {
                    this.doDisplayDefaultTemplate = false;
                    this.detailsTemplateDefinition = null;
                    this.changeDetectorRef.detectChanges();
                    this.changeDetectorRef.detach();
                }),
                switchMap(() => this.route.data),
                distinctUntilChanged(), // need to make sure we get the new route data
                switchMap(d => this.appSettings.getSettings().pipe(map(s => ({ settings: s, resolvedPackage: d.resolvedPackage })))),
                switchMap((data: { settings: ApplicationSettings, resolvedPackage: ResolvedPackage }) => {
                    if (!data.resolvedPackage) {
                        throw new Error('Missing expected route resolve data: resolvedTemplateCard');
                    }

                    if (data.resolvedPackage.errorMessage) {
                        throw new Error(data.resolvedPackage.errorMessage);
                    }

                    const packageData = data.resolvedPackage.data;

                    const previewImages = packageData.displayInfo && packageData.displayInfo.previewImages || [];
                    const packageDescription =
                        packageData.displayInfo && packageData.displayInfo.descriptionParagraphs || [packageData.abstract];
                    const packageDescriptor = packageData.displayInfo && packageData.displayInfo.siteDescriptor;
                    const templateId = packageData.displayInfo ? packageData.displayInfo.pageTemplateId : null;
                    const detailItemCategories = packageData.displayInfo && packageData.displayInfo.detailItemCategories || [];
                    const systemRequirements = packageData.displayInfo && packageData.displayInfo.systemRequirements || [];

                    this.pageData = {
                        packageId: packageData.id,
                        packageTitle: packageData.displayName,
                        packageType: packageData.packageType,
                        provisioningFormUrl: packageData.provisioningFormUrl,
                        packageDescriptor,
                        packageDescription,
                        previewImages,
                        detailItemCategories,
                        systemRequirements,
                        telemetryUrl: data.settings.telemetryUrl + packageData.displayName
                    };

                    this.titleService.setTitle(`${appConstants.getSiteTitle(data.settings.targetPlatformId)} - ${packageData.displayName}`);    
                    this.trackingService.track(this.route, { TemplateId: packageData.id });

                    return this.loadTemplate(templateId);
                }),
                catchError((err: Error) => {
                    this.changeDetectorRef.reattach();
                    this.snackBar.open(err.message, 'Dismiss');
                    return EMPTY;
                }),
                tap(() => this.changeDetectorRef.reattach())
            )
            .subscribe();
    }

    ngOnDestroy() {
        this.componentDestroy.next();
        this.componentDestroy.complete();
    }

    handleDyanmicComponentError() {
        this.doDisplayDefaultTemplate = true;
        this.changeDetectorRef.detectChanges();
    }

    private loadTemplate(templateId: string) {
        if (!templateId) {
            return this.handleTemplateError();
        }

        return this.pageTemplateService.getPageTemplate(templateId)
            .pipe(
                map(pageTemplate => {
                    this.detailsTemplateDefinition = {
                        moduleDefinition: detailsModuleDefinition,
                        html: pageTemplate.html,
                        css: pageTemplate.css,
                        context: this.pageData
                    };
                }),
                catchError(() => this.handleTemplateError())
            );
    }

    private handleTemplateError() {
        this.doDisplayDefaultTemplate = true;
        this.changeDetectorRef.reattach();
        return EMPTY;
    }
}
