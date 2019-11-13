import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { DetailsPageTemplateDataService } from '../details-page-template-data.service';

@Component({
    // tslint:disable-next-line: component-selector
    selector: 'slb-package-title',
    templateUrl: './package-title.component.html',
})
export class PackageTitleComponent implements OnInit {
    packageTitle: Observable<string>;

    constructor(
        private dataService: DetailsPageTemplateDataService
    ) { }

    ngOnInit() {
        this.packageTitle = this.dataService.data
            .pipe(map(data => data.packageTitle));
    }
}
