import { Component, OnInit, TemplateRef, ViewChild, ViewContainerRef, ChangeDetectionStrategy } from '@angular/core';
import { TemplatePortal } from '@angular/cdk/portal';

@Component({
    // tslint:disable-next-line: component-selector
    selector: 'slb-carousel-item',
    templateUrl: './carousel-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CarouselItemComponent implements OnInit {
    /**
     * Reference to the template containing the content so we can make a Template Portal
     */
    @ViewChild(TemplateRef, { static: true }) contentTemplate: TemplateRef<any>;

    /**
     * The template portal to use in the carousel item body
     */
    content: TemplatePortal;

    /**
     * If the item is currently the active item
     */
    isActive: boolean;

    constructor(
        private viewContainer: ViewContainerRef
    ) { }

    ngOnInit() {
        this.content = new TemplatePortal(this.contentTemplate, this.viewContainer);
    }
}
