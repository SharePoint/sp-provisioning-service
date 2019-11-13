import { Component, OnInit, HostBinding } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { DetailsPageTemplateDataService } from '../details-page-template-data.service';

const componentSelector = 'slb-permission-message';

@Component({
    // tslint:disable-next-line: component-selector
    selector: componentSelector,
    templateUrl: './permission-message.component.html',
})
export class PermissionMessageComponent implements OnInit {
    packageType: Observable<string>;
    @HostBinding(`class.${componentSelector}`) cssClass = true;

    packageTypes = {
        tenant: 'Tenant',
        siteCollection: 'SiteCollection'
    };

    constructor(
        private dataService: DetailsPageTemplateDataService
    ) { }

    ngOnInit() {
        this.packageType = this.dataService.data
            .pipe(map(data => data.packageType));
    }
}
