import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { DetailsPageTemplateDataService } from '../details-page-template-data.service';

@Component({
    // tslint:disable-next-line: component-selector
    selector: 'slb-package-description',
    templateUrl: './package-description.component.html',
})
export class PackageDescriptionComponent implements OnInit {
    packageDescriptionParagraphs: Observable<string[]>;

    constructor(
        private dataService: DetailsPageTemplateDataService
    ) { }

    ngOnInit() {
        this.packageDescriptionParagraphs = this.dataService.data
            .pipe(map(data => data.packageDescription));
    }
}
