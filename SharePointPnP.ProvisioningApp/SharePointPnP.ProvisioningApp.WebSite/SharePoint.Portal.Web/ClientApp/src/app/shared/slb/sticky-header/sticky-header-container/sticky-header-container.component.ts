import { Component, HostBinding, ComponentRef, EmbeddedViewRef, ViewChild } from '@angular/core';
import { BasePortalOutlet, ComponentPortal, TemplatePortal, CdkPortalOutlet } from '@angular/cdk/portal';

const componentSelector = 'slb-sticky-header-container';

@Component({
    selector: componentSelector,
    templateUrl: './sticky-header-container.component.html',
})
export class StickyHeaderContainerComponent extends BasePortalOutlet {
    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;

    @ViewChild(CdkPortalOutlet, { static: true }) private portalOutlet: CdkPortalOutlet;

    constructor() {
        super();
    }

    attachComponentPortal<T>(portal: ComponentPortal<T>): ComponentRef<T> {
        if (this.portalOutlet.hasAttached()) {
            throw new Error('Content already attached');
        }
        return this.portalOutlet.attachComponentPortal(portal);
    }

    attachTemplatePortal<C>(portal: TemplatePortal<C>): EmbeddedViewRef<C> {
        if (this.portalOutlet.hasAttached()) {
            throw new Error('Content already attached');
        }
        return this.portalOutlet.attachTemplatePortal(portal);
    }
}
