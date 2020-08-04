import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Title } from '@angular/platform-browser';
import { Observable, EMPTY } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { appConstants } from '../app-constants';
import { Category } from '../core/api/models';
import { CategoriesService, TrackingService } from '../core/api/services';

import { map } from 'rxjs/operators';
import { ApplicationSettingsService } from '../core/api/services/application-settings.service';
import { ActivatedRoute } from '@angular/router';
import { environment } from 'src/environments/environment';

interface Quote {
    quoteText: string;
    customerName: string;
}

const quotes: Quote[] = [
    {
        quoteText: `What three years ago took months of coding is now available out of the box with SharePoint. I think that's amazing.`,
        customerName: 'Stig Thomsen, Lead Architect, My Digital Workspace, Arla Foods',    },
    {
        // tslint:disable-next-line: max-line-length
        quoteText: `It's simply fun to work with the communication sites in SharePoint Online. The design is appealing, and the handling when creating content is simple and self-explanatory.`,
        customerName: 'Klöckner & Co employee'
    },
    {
        // tslint:disable-next-line: max-line-length
        quoteText: `We get new capabilities without the extra cost of—or months waiting on—developers because we chose an evergreen solution like SharePoint Online.`,
        customerName: 'Harris Medović, Enterprise Solutions Architect, VELUX Group'
    },
];

@Component({
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
    targetPlatformId: Observable<string>;

    categories: Observable<Category[]>;
    quotes: Quote[];

    telemetryUrl: Observable<string>;

    constructor(
        private route: ActivatedRoute,
        private trackingService: TrackingService,
        private categoriesService: CategoriesService,
        private appSettings: ApplicationSettingsService,
        private matSnackBar: MatSnackBar,
        private titleService: Title,
    ) { }

    ngOnInit() {
        this.quotes = quotes;

        this.trackingService.track(this.route);

        this.categories = this.categoriesService.getAll()
            .pipe(catchError(() => {
                this.matSnackBar.open('There was an error loading the examples, please reload the page.', 'Dismiss');
                return EMPTY;
            }));

        this.targetPlatformId = this.appSettings
            .getSettings()
            .pipe(map(settings => settings.targetPlatformId));

        this.telemetryUrl = this.appSettings
            .getSettings()
            .pipe(map(settings => settings.telemetryUrl + 'home'));

        this.targetPlatformId
            .subscribe(s => this.titleService.setTitle(appConstants.getSiteTitle(s)));
    }
}
