import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllMicrosoftMegamenuComponent } from './all-microsoft-megamenu.component';
import { NavMenuActivatorDirective } from '../nav-menu-activator.directive';
import { NavMenuPanelDirective } from '../nav-menu-panel.directive';

describe('AllMicrosoftMegamenuComponent', () => {
    let component: AllMicrosoftMegamenuComponent;
    let fixture: ComponentFixture<AllMicrosoftMegamenuComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [
                AllMicrosoftMegamenuComponent,
                NavMenuActivatorDirective,
                NavMenuPanelDirective
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(AllMicrosoftMegamenuComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => expect(component).toBeTruthy());
});
