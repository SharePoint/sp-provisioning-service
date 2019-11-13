import { Component, ViewChild, ElementRef } from '@angular/core';
import { TestBed, ComponentFixture, async } from '@angular/core/testing';

import { SlbStickyHeaderModule } from './sticky-header.module';
import { StickyHeaderContentClassDirective } from './sticky-header-content-class.directive';

@Component({
    template: `
    <div [slbStickyHeaderContentClass]="cssClass" #element>
    `
})
class StickyHeaderContentClassTestComponent {
    @ViewChild('element', { static: true }) directiveElement: ElementRef;
    @ViewChild(StickyHeaderContentClassDirective, { static: true }) directive: StickyHeaderContentClassDirective;
    cssClass: string;
}

describe('StickyHeaderContentDirective', () => {
    let component: StickyHeaderContentClassTestComponent;
    let fixture: ComponentFixture<StickyHeaderContentClassTestComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [SlbStickyHeaderModule],
            declarations: [StickyHeaderContentClassTestComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(StickyHeaderContentClassTestComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create an instance', () => expect(component).toBeTruthy());

    it('should add/remove the class', () => {
        const elem = component.directiveElement.nativeElement as HTMLElement;
        component.cssClass = 'test-class';
        fixture.detectChanges();

        component.directive.markStuck();
        expect(elem.classList.contains('test-class')).toBe(true);

        component.directive.markUnstuck();
        expect(elem.classList.contains('test-class')).toBe(false);
    });

    it('should update the class', () => {
        const elem = component.directiveElement.nativeElement as HTMLElement;
        component.cssClass = 'test-class';
        fixture.detectChanges();

        component.directive.markStuck();
        expect(elem.classList.contains('test-class')).toBe(true);

        component.cssClass = 'new-class';
        fixture.detectChanges();
        expect(elem.classList.contains('test-class')).toBe(false);
        expect(elem.classList.contains('new-class')).toBe(true);
    });

    it('should not update the class if it is not stuck', () => {
        const elem = component.directiveElement.nativeElement as HTMLElement;

        component.cssClass = 'test-class';
        fixture.detectChanges();
        expect(elem.classList.contains('test-class')).toBe(false);

        component.cssClass = 'new-class';
        fixture.detectChanges();
        expect(elem.classList.contains('test-class')).toBe(false);
        expect(elem.classList.contains('new-class')).toBe(false);
    });
});
