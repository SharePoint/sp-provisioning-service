import { Directive, Input } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BreakpointObserver, BreakpointState } from '@angular/cdk/layout';
import { By } from '@angular/platform-browser';
import { Subject } from 'rxjs';

import { DocumentClickService } from '../document-click.service';
import { CategoriesService } from '../api/services';

import { Category } from '../api/models';
import { NavMenuItemComponent } from './nav-menu-item/nav-menu-item.component';
import { AllMicrosoftMegamenuComponent } from './all-microsoft-megamenu/all-microsoft-megamenu.component';
import { HeaderComponent } from './header.component';

@Directive({
    selector: '[appNavMenuActivatorFor]'
})
class MenuActivatorStubDirective {
    @Input('appNavMenuActivatorFor') menu;
}

@Directive({
    selector: '[appNavMenuPanel]',
    exportAs: 'navMenuPanel'
})
class MenuPanelStubDirective {
    toggle() { }
    open() { }
    close() { }
}

describe('HeaderComponent', () => {
    let component: HeaderComponent;
    let fixture: ComponentFixture<HeaderComponent>;

    let categorySubject: Subject<Category[]>;
    let breakPointSubject: Subject<BreakpointState>;
    let documentClickSubject: Subject<{ target: HTMLElement }>;

    beforeEach(async(() => {
        categorySubject = new Subject<Category[]>();
        const categoryServiceSpy = jasmine.createSpyObj('CategoriesService', ['getAll']);
        categoryServiceSpy.getAll.and.returnValue(categorySubject);

        breakPointSubject = new Subject<BreakpointState>();
        const breakpointObserverSpy = jasmine.createSpyObj('BreakpointObserver', ['observe', 'isMatched']);
        breakpointObserverSpy.observe.and.returnValue(breakPointSubject);

        documentClickSubject = new Subject<{ target: HTMLElement }>();
        const documentClickServiceMock = {
            click: documentClickSubject
        };

        TestBed.configureTestingModule({
            declarations: [
                HeaderComponent,
                NavMenuItemComponent,
                AllMicrosoftMegamenuComponent,
                MenuActivatorStubDirective,
                MenuPanelStubDirective,
            ],
            imports: [
                RouterTestingModule
            ],
            providers: [
                { provide: CategoriesService, useValue: categoryServiceSpy },
                { provide: BreakpointObserver, useValue: breakpointObserverSpy },
                { provide: DocumentClickService, useValue: documentClickServiceMock }
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(HeaderComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => expect(component).toBeTruthy());

    it('should close the menu if clicking outside of the menu area', () => {
        component.isShowingMainMenu = true;

        const insideElem = fixture.debugElement.query(By.css('.collapsed-nav-opener')).nativeElement;
        documentClickSubject.next({ target: insideElem });
        expect(component.isShowingMainMenu).toBe(true);

        const outsideElem = fixture.debugElement.query(By.css('.expanded-home-nav-item')).nativeElement;
        documentClickSubject.next({ target: outsideElem });
        expect(component.isShowingMainMenu).toBe(false);
    });
});
