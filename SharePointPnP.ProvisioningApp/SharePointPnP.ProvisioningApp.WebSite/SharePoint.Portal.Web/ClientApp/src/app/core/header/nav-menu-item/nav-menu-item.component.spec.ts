import { Directive, Input, Component, ViewChild } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { By } from '@angular/platform-browser';

import { NavMenuPanelDirective } from '../nav-menu-panel.directive';
import { NavMenuActivatorDirective } from '../nav-menu-activator.directive';
import { NavMenuItem } from './nav-menu-item';
import { NavMenuItemComponent } from './nav-menu-item.component';

@Component({
    template: `<app-nav-menu-item [item]="item"></app-nav-menu-item>`
})
class NavMenuItemTestComponent {
    item: NavMenuItem;
    @ViewChild(NavMenuItemComponent, { static: false }) component: NavMenuItemComponent;
}

describe('NavMenuItemComponent', () => {
    let itemWithSubItems: NavMenuItem;

    beforeEach(async(() => {
        itemWithSubItems = {
            name: 'Parent Item',
            subItems: [{ name: 'Subitem', routerLink: [] }]
        };

        TestBed.configureTestingModule({
            declarations: [
                NavMenuItemComponent,
                NavMenuItemTestComponent,
                NavMenuActivatorDirective,
                NavMenuPanelDirective
            ],
            imports: [
                RouterTestingModule,
            ]
        })
            .compileComponents();
    }));

    describe('simple tests', () => {
        let component: NavMenuItemComponent;
        let fixture: ComponentFixture<NavMenuItemComponent>;

        beforeEach(() => {
            fixture = TestBed.createComponent(NavMenuItemComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should create', () => expect(component).toBeTruthy());

        it('should start closed', () => expect(component.isOpen).toBeFalsy());

        it('should emit when panel opened', () => {
            const spy = jasmine.createSpy('openCallbackSpy');
            component.opened.subscribe(() => spy());

            component.handlePanelOpened();

            expect(spy).toHaveBeenCalled();
        });

        it('should emit when panel closed', () => {
            const spy = jasmine.createSpy('closeCallbackSpy');
            component.closed.subscribe(() => spy());

            component.handlePanelClosed();

            expect(spy).toHaveBeenCalled();
        });
    });

    describe('item tests', () => {
        // need to wrap the component because we are using OnPush change detection
        let component: NavMenuItemTestComponent;
        let fixture: ComponentFixture<NavMenuItemTestComponent>;

        beforeEach(() => {
            fixture = TestBed.createComponent(NavMenuItemTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should display a link for an item without subitems set', () => {
            component.item = { name: 'Test' };
            fixture.detectChanges();

            const link = fixture.debugElement.query(By.css('a'));
            expect(link).toBeTruthy();

            const button = fixture.debugElement.query(By.css('button'));
            expect(button).toBeFalsy();
        });

        it('should display a link for an item with empty subitems set', () => {
            component.item = { name: 'Test', subItems: [] };
            fixture.detectChanges();

            const link = fixture.debugElement.query(By.css('a'));
            expect(link).toBeTruthy();

            const button = fixture.debugElement.query(By.css('button'));
            expect(button).toBeFalsy();
        });

        it('should display a button to open a sub menu if there are sub items', () => {
            component.item = {
                name: 'Parent Item',
                subItems: [
                    { name: 'Sub item' }
                ]
            };
            fixture.detectChanges();

            const button = fixture.debugElement.query(By.css('button'));
            expect(button).toBeTruthy();

            const menuPanel = fixture.debugElement.query(By.css('.menu-panel')).nativeElement as HTMLElement;
            expect(menuPanel).toBeTruthy();
        });

        it('should toggle open when the menu button is clicked', () => {
            component.item = {
                name: 'Parent Item',
                subItems: [
                    { name: 'Sub item' }
                ]
            };
            fixture.detectChanges();
            const button = fixture.debugElement.query(By.css('button'));

            button.nativeElement.click();
            fixture.detectChanges();
            expect(component.component.isOpen).toBeTruthy();

            button.nativeElement.click();
            fixture.detectChanges();
            expect(component.component.isOpen).toBeFalsy();
        });
    });
});
