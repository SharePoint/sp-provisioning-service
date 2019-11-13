import { Component, ViewChild } from '@angular/core';
import { TestBed, async, ComponentFixture } from '@angular/core/testing';

import { NavMenuActivatorDirective } from './nav-menu-activator.directive';
import { By } from '@angular/platform-browser';
import { NavMenuPanelDirective } from './nav-menu-panel.directive';

@Component({
    template: '<div [appNavMenuActivatorFor]="menu">'
})
class NavMenuActivatorTestComponent {
    @ViewChild(NavMenuActivatorDirective, { static: false }) directive: NavMenuActivatorDirective;
    menu = new NavMenuPanelDirective();
}

describe('NavMenuActivatorDirective', () => {
    let fixture: ComponentFixture<NavMenuActivatorTestComponent>;
    let component: NavMenuActivatorTestComponent;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [
                NavMenuActivatorDirective,
                NavMenuActivatorTestComponent
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(NavMenuActivatorTestComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create an instance', () => expect(component.directive).toBeTruthy());

    it('should add the glyph class', () => {
        const elem = fixture.debugElement.query(By.directive(NavMenuActivatorDirective)).nativeElement as HTMLElement;
        expect(elem.classList.contains('has-glyph')).toBe(true);
    });

    it('should apply the open class if the menu is open', () => {
        const elem = fixture.debugElement.query(By.directive(NavMenuActivatorDirective)).nativeElement as HTMLElement;

        component.menu.isOpen = false;
        fixture.detectChanges();
        expect(elem.classList.contains('is-open')).toBe(false);

        component.menu.isOpen = true;
        fixture.detectChanges();
        expect(elem.classList.contains('is-open')).toBe(true);
    });

    it('should call toggle when clicked', () => {
        const toggleSpy = spyOn(component.menu, 'toggle');
        const elem = fixture.debugElement.query(By.directive(NavMenuActivatorDirective));

        elem.nativeElement.click();
        expect(toggleSpy).toHaveBeenCalledTimes(1);

        elem.nativeElement.click();
        expect(toggleSpy).toHaveBeenCalledTimes(2);
    });
});
