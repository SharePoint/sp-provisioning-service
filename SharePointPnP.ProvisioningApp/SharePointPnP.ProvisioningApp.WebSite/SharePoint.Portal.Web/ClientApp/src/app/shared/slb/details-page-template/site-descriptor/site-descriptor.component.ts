import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { DetailsPageTemplateDataService } from '../details-page-template-data.service';

@Component({
    // tslint:disable-next-line: component-selector
    selector: 'slb-site-descriptor',
    templateUrl: './site-descriptor.component.html',
})
export class SiteDescriptorComponent implements OnInit {
    siteDescriptor: Observable<string>;

    constructor(
        private dataService: DetailsPageTemplateDataService
    ) { }

    ngOnInit() {
        this.siteDescriptor = this.dataService.data
            .pipe(map(data => data.packageDescriptor));
    }
}
