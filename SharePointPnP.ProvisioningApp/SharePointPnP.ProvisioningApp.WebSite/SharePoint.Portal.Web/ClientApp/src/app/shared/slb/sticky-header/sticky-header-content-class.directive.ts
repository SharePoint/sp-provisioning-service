import { Directive, Input, ElementRef, Renderer2, OnChanges, OnDestroy } from '@angular/core';

/**
 * Adds class to the element when the header is stickied.
 */

@Directive({
    // tslint:disable-next-line: directive-selector
    selector: '[slbStickyHeaderContentClass]'
})
export class StickyHeaderContentClassDirective implements OnChanges, OnDestroy {
    @Input('slbStickyHeaderContentClass') stickiedClass: string;

    private isStuck = false;
    private classAdded: string;

    constructor(
        private elementRef: ElementRef,
        private renderer: Renderer2
    ) { }

    ngOnChanges() {
        if (this.isStuck) {
            if (this.stickiedClass !== this.classAdded) {
                this.removeStickyClass();
                this.addStickyClass();
            }
        }
    }

    ngOnDestroy() {
        this.removeStickyClass();
    }

    markStuck() {
        this.addStickyClass();
        this.isStuck = true;
    }

    markUnstuck() {
        this.removeStickyClass();
        this.isStuck = false;
    }

    private addStickyClass() {
        if (this.stickiedClass) {
            this.stickiedClass.split(' ')
                .forEach(cssClass => this.renderer.addClass(this.elementRef.nativeElement, cssClass));
        }
        this.classAdded = this.stickiedClass;
    }

    private removeStickyClass() {
        if (this.classAdded) {
            this.classAdded.split(' ')
                .forEach(cssClass => this.renderer.removeClass(this.elementRef.nativeElement, cssClass));
        }
        this.classAdded = null;
    }
}
