import { Component, Input, HostBinding } from '@angular/core';
import { TemplatePortal } from '@angular/cdk/portal';

const componentSelector = 'slb-tab-body';

@Component({
    selector: componentSelector,
    templateUrl: './tab-body.component.html',
    styleUrls: ['./tab-body.component.scss']
})
export class TabBodyComponent {
    @Input() content: TemplatePortal;
    @Input() isActive: boolean;

    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;
}
