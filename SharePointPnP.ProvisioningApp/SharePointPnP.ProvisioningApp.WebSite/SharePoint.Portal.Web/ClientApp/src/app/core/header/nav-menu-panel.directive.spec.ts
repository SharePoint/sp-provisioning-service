import { Component, ViewChild } from '@angular/core';
import { TestBed, async, ComponentFixture } from '@angular/core/testing';

import { NavMenuPanelDirective } from './nav-menu-panel.directive';
import { By } from '@angular/platform-browser';

@Component({
    template: '<div appNavMenuPanel>'
})
class NavMenuPanelTestComponent {
    @ViewChild(NavMenuPanelDirective, { static: true }) directive: NavMenuPanelDirective;
}

describe('NavMenuPanelDirective', () => {
    let fixture: ComponentFixture<NavMenuPanelTestComponent>;
    let component: NavMenuPanelTestComponent;
    let directive: NavMenuPanelDirective;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [
                NavMenuPanelDirective,
                NavMenuPanelTestComponent
            ]
        });
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(NavMenuPanelTestComponent);
        component = fixture.componentInstance;
        directive = component.directive;
        fixture.detectChanges();
    });

    it('should create', () => expect(directive).toBeTruthy());

    it('should start closed', () => expect(directive.isOpen).toBe(false));

    it('should apply the panel class', () => {
        const elem = fixture.debugElement.query(By.directive(NavMenuPanelDirective)).nativeElement as HTMLElement;
        expect(elem.classList.contains('menu-panel')).toBe(true);
    });

    it('should apply the open class when panel is open', () => {
        directive.open();
        fixture.detectChanges();

        expect(directive.isOpen).toBe(true);

        const elem = fixture.debugElement.query(By.directive(NavMenuPanelDirective)).nativeElement as HTMLElement;

        expect(elem.classList.contains('is-open')).toBe(true);

        directive.close();
        fixture.detectChanges();

        expect(elem.classList.contains('is-open')).toBe(false);
    });

    it('should emit when opened', () => {
        const spy = jasmine.createSpy('openCallbackSpy');
        directive.opened.subscribe(() => spy());

        directive.open();
        expect(spy).toHaveBeenCalled();

        spy.calls.reset();
        directive.open();
        expect(spy).not.toHaveBeenCalled();
    });

    it('should emit when closed', () => {
        const spy = jasmine.createSpy('closeCallbackSpy');
        directive.closed.subscribe(() => spy());
        directive.isOpen = true;

        directive.close();
        expect(spy).toHaveBeenCalled();

        spy.calls.reset();
        directive.close();
        expect(spy).not.toHaveBeenCalled();
    });
});
