import { Component, Input, HostBinding } from '@angular/core';
import { DetailItem } from 'src/app/core/api/models';

const componentSelector = 'slb-detail-item';

@Component({
    selector: componentSelector,
    templateUrl: './detail-item.component.html',
    styleUrls: ['./detail-item.component.scss']
})
export class DetailItemComponent {
    @Input() item: DetailItem;
    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;
}
