import { Directive, OnInit, OnChanges, Input } from '@angular/core';

import { DetailsPageTemplateDataService, DetailsPageTemplateData } from './details-page-template-data.service';

const directiveSelector = 'slbDetailsPageTemplateContext';

@Directive({
    selector: `[${directiveSelector}]`,
    providers: [DetailsPageTemplateDataService]
})
export class DetailsTemplateContextDirective implements OnInit, OnChanges {
    @Input(directiveSelector) dataContext: DetailsPageTemplateData;

    constructor(
        private dataService: DetailsPageTemplateDataService
    ) { }

    ngOnInit() {
        this.dataService.setDetailData(this.dataContext);
    }

    ngOnChanges() {
        this.dataService.setDetailData(this.dataContext);
    }
}
