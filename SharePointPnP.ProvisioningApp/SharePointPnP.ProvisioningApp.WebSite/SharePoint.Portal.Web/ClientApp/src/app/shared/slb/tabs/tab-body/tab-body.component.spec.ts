import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { TemplatePortal, PortalModule } from '@angular/cdk/portal';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';

import { TabBodyComponent } from './tab-body.component';

describe('TabBodyComponent', () => {
    let component: TabBodyComponent;
    let fixture: ComponentFixture<TabBodyComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [
                PortalModule,
                NoopAnimationsModule
            ],
            declarations: [
                TabBodyComponent
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(TabBodyComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => expect(component).toBeTruthy());

    it('should be hidden when not active', () => {
        const contentElem = fixture.debugElement.query(By.css('div'));

        component.isActive = false;
        fixture.detectChanges();

        expect(contentElem.properties.hidden).toBeTruthy();

        component.isActive = true;
        fixture.detectChanges();

        expect(contentElem.properties.hidden).not.toBeTruthy();
    });
});
