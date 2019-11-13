import { Component, OnInit, HostBinding, Input, ChangeDetectionStrategy } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { SystemRequirement } from 'src/app/core/api/models';
import { DetailsPageTemplateDataService } from '../details-page-template-data.service';

const componentSelector = 'slb-system-requirements-table';

@Component({
    selector: componentSelector,
    templateUrl: './system-requirements-table.component.html',
    styleUrls: ['./system-requirements-table.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SystemRequirementsTableComponent implements OnInit {
    @Input() noRequirementsMessage: string;
    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;

    systemRequirements: Observable<SystemRequirement[]>;
    hasLoaded: boolean;

    constructor(
        private dataService: DetailsPageTemplateDataService
    ) { }

    ngOnInit() {
        this.systemRequirements = this.dataService.data
            .pipe(
                map(data => {
                    this.hasLoaded = true;
                    return data.systemRequirements;
                })
            );

        this.noRequirementsMessage = this.noRequirementsMessage || 'No requirements defined.';
    }
}
