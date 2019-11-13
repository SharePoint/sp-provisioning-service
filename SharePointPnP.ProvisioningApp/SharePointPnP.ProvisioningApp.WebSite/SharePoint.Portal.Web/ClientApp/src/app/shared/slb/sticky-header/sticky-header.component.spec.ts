import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ScrollDispatcher, Overlay, OverlayRef } from '@angular/cdk/overlay';
import { Component, ElementRef } from '@angular/core';
import { By } from '@angular/platform-browser';
import { Subject } from 'rxjs';

import { SlbStickyHeaderModule } from './sticky-header.module';
import { StickyHeaderComponent } from './sticky-header.component';

@Component({
    template: `
    <slb-sticky-header (stickied)="stickied = $event">
        {{ contents }}
    </slb-sticky-header>
    `
})
class StickyHeaderTestComponent {
    stickied: boolean;
    contents: string;
}

describe('StickyHeaderComponent', () => {
    let scrolledSubject: Subject<void>;

    beforeEach(async(() => {
        scrolledSubject = new Subject();

        TestBed.configureTestingModule({
            imports: [
                SlbStickyHeaderModule
            ],
            declarations: [
                StickyHeaderTestComponent
            ],
            providers: [
                { provide: ScrollDispatcher, useFactory: () => ({ scrolled: () => scrolledSubject.asObservable() }) },
            ]
        })
            .compileComponents();
    }));

    describe('simple tests', () => {
        let component: StickyHeaderComponent;
        let fixture: ComponentFixture<StickyHeaderComponent>;

        beforeEach(() => {
            fixture = TestBed.createComponent(StickyHeaderComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should create', () => expect(component).toBeTruthy());

        it('should stick the header and keep it stuck', () => {
            const overlay: Overlay = TestBed.get(Overlay);
            const overlayCreateSpy = spyOn(overlay, 'create').and.callThrough();
            const elementRef: ElementRef = fixture.componentRef.injector.get(ElementRef);
            spyOn(elementRef.nativeElement, 'getBoundingClientRect').and.returnValue({ top: -1 });

            scrolledSubject.next();
            expect(overlayCreateSpy).toHaveBeenCalledTimes(1);

            scrolledSubject.next();
            expect(overlayCreateSpy).toHaveBeenCalledTimes(1);

            scrolledSubject.next();
            expect(overlayCreateSpy).toHaveBeenCalledTimes(1);
        });

        it('should not stick the header if at top of page', () => {
            const overlay: Overlay = TestBed.get(Overlay);
            const overlayCreateSpy = spyOn(overlay, 'create').and.callThrough();
            const elementRef: ElementRef = fixture.componentRef.injector.get(ElementRef);
            spyOn(elementRef.nativeElement, 'getBoundingClientRect').and.returnValue({ top: 0 });

            overlayCreateSpy.calls.reset();

            scrolledSubject.next();
            expect(overlayCreateSpy).not.toHaveBeenCalled();

            scrolledSubject.next();
            expect(overlayCreateSpy).not.toHaveBeenCalled();
        });

        it('should stick and unstick a header', () => {
            const overlay: Overlay = TestBed.get(Overlay);
            const overlayRefSpy: jasmine.SpyObj<OverlayRef> = jasmine.createSpyObj('OverlayRef', ['attach', 'dispose']);
            overlayRefSpy.attach.and.returnValue({
                instance: jasmine.createSpyObj('StickyHeaderContainerComponent', ['attachTemplatePortal'])
            });
            spyOn(overlay, 'create').and.returnValue(overlayRefSpy);
            const elementRef: ElementRef = fixture.componentRef.injector.get(ElementRef);
            const boundingClientRectSpy = spyOn(elementRef.nativeElement, 'getBoundingClientRect');

            overlayRefSpy.attach.calls.reset();
            overlayRefSpy.dispose.calls.reset();

            boundingClientRectSpy.and.returnValue({ top: -1 });
            scrolledSubject.next();
            expect(overlayRefSpy.attach).toHaveBeenCalled();

            boundingClientRectSpy.and.returnValue({ top: 0 });
            scrolledSubject.next();
            expect(overlayRefSpy.dispose).toHaveBeenCalled();
        });
    });

    describe('content tests', () => {
        let component: StickyHeaderTestComponent;
        let fixture: ComponentFixture<StickyHeaderTestComponent>;

        beforeEach(() => {
            fixture = TestBed.createComponent(StickyHeaderTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should have the unstuck contents', () => {
            const contents = 'Contents of header';
            component.contents = contents;
            fixture.detectChanges();

            expect(fixture.debugElement.nativeElement.innerText).toBe(contents);
        });

        it('should update the status', () => {
            const stickyHeader = fixture.debugElement.query(By.directive(StickyHeaderComponent));
            const elementRef: ElementRef = stickyHeader.injector.get(ElementRef);
            const boundingClientRectSpy = spyOn(elementRef.nativeElement, 'getBoundingClientRect');

            boundingClientRectSpy.and.returnValue({ top: -1 });
            scrolledSubject.next();
            expect(component.stickied).toBe(true);

            boundingClientRectSpy.and.returnValue({ top: 0 });
            scrolledSubject.next();
            expect(component.stickied).toBe(false);
        });
    });
});
