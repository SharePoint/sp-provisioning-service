import { Component, OnInit, ViewChild, TemplateRef, ViewContainerRef, Input, ChangeDetectionStrategy } from '@angular/core';
import { TemplatePortal } from '@angular/cdk/portal';

@Component({
    // tslint:disable-next-line: component-selector
    selector: 'slb-tab',
    templateUrl: './tab.component.html',
    styleUrls: ['./tab.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TabComponent implements OnInit {
    @Input() label: string;

    /**
     * Reference to the template containing the content so we can make a Template Portal
     */
    @ViewChild(TemplateRef, { static: true }) contentTemplate: TemplateRef<any>;

    /**
     * The template portal to use in the carousel item body
     */
    content: TemplatePortal;

    /**
     * If the tab is currently active
     */
    isActive: boolean;

    constructor(
        private viewContainer: ViewContainerRef
    ) { }

    ngOnInit() {
        this.content = new TemplatePortal(this.contentTemplate, this.viewContainer);
    }
}
