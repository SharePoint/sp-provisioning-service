import {
    Component, OnInit, AfterContentInit, OnDestroy, Output,
    TemplateRef, ViewContainerRef, ElementRef, Renderer2, EventEmitter, NgZone, Injector,
    ViewChild, ContentChildren, QueryList
} from '@angular/core';
import { TemplatePortal, CdkPortalOutlet, ComponentPortal } from '@angular/cdk/portal';
import { Overlay, ScrollDispatcher, OverlayRef } from '@angular/cdk/overlay';
import { takeUntil, filter, take } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { StickyHeaderContainerComponent } from './sticky-header-container/sticky-header-container.component';
import { StickyHeaderContentClassDirective } from './sticky-header-content-class.directive';

const componentSelector = 'slb-sticky-header';

@Component({
    selector: componentSelector,
    templateUrl: './sticky-header.component.html',
    styleUrls: ['./sticky-header.component.scss']
})
export class StickyHeaderComponent implements OnInit, OnDestroy, AfterContentInit {
    @Output() stickied = new EventEmitter<boolean>();

    @ViewChild('template', { static: true }) private contentTemplate: TemplateRef<any>;
    @ViewChild(CdkPortalOutlet, { static: true }) private portalOutlet: CdkPortalOutlet;
    @ContentChildren(StickyHeaderContentClassDirective) private contentChildren: QueryList<StickyHeaderContentClassDirective>;
    private overlayRef: OverlayRef;
    private content: TemplatePortal;

    private componentDestroy = new Subject<void>();

    constructor(
        private elementRef: ElementRef,
        private injector: Injector,
        private ngZone: NgZone,
        private overlay: Overlay,
        private renderer: Renderer2,
        private scrollDispatcher: ScrollDispatcher,
        private viewContainer: ViewContainerRef,
    ) { }

    ngOnInit() {
        this.content = new TemplatePortal(this.contentTemplate, this.viewContainer);
    }

    ngAfterContentInit() {
        this.ngZone.onStable
            .pipe(take(1))
            .subscribe(() => this.updateStickiness());

        this.scrollDispatcher.scrolled()
            .pipe(
                takeUntil(this.componentDestroy),
                filter(scrollable => !scrollable), // we want the global scroll
            )
            .subscribe(() => this.updateStickiness());
    }

    ngOnDestroy() {
        if (this.overlayRef) {
            this.overlayRef.dispose();
        }
        this.componentDestroy.next();
        this.componentDestroy.complete();
    }

    private updateStickiness() {
        const elementTop = this.elementRef.nativeElement.getBoundingClientRect().top;
        if (elementTop < 0) {
            if (!this.overlayRef || !this.overlayRef.hasAttached()) {
                this.stickHeader();
            }
        } else if (!this.portalOutlet.hasAttached()) {
            this.unstickHeader();
        }
    }

    private stickHeader() {
        if (this.overlayRef && this.overlayRef.hasAttached()) {
            throw new Error('Sticky header has already been stuck');
        }

        this.renderer.setStyle(this.elementRef.nativeElement, 'height', this.elementRef.nativeElement.offsetHeight + 'px');
        this.portalOutlet.detach();

        this.overlayRef = this.overlay.create({ width: '100%' });
        const headerContainer = new ComponentPortal(StickyHeaderContainerComponent, this.viewContainer, this.injector);
        const containerRef = this.overlayRef.attach(headerContainer);
        containerRef.instance.attachTemplatePortal(this.content);

        this.contentChildren.forEach(item => item.markStuck());

        // scroll event is run out of zone, so we must bring this back in the zone
        this.ngZone.run(() => {
            this.stickied.emit(true);
        });
    }

    private unstickHeader() {
        if (this.portalOutlet.hasAttached()) {
            throw new Error('Sticky header is already unstuck');
        }

        if (this.overlayRef) {
            this.overlayRef.dispose();
        }
        this.renderer.removeStyle(this.elementRef.nativeElement, 'height');
        this.portalOutlet.attach(this.content);

        this.contentChildren.forEach(item => item.markUnstuck());

        // scroll event is run out of zone, so we must bring this back in the zone
        this.ngZone.run(() => {
            this.stickied.emit(false);
        });
    }
}
