import { Component, ViewChild } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { ExpandablePanelComponent } from './expandable-panel/expandable-panel.component';
import { SlbExpandablePanelModule } from './expandable-panel.module';

@Component({
    template: `
    <slb-expandable-panel [(expanded)]="expanded">
        <slb-expandable-panel-header>Header</slb-expandable-panel-header>
    </slb-expandable-panel>`
})
class PanelTestComponent {
    expanded = false;

    @ViewChild(ExpandablePanelComponent, { static: true }) component: ExpandablePanelComponent;
}

describe('ExpandablePanelComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [
                PanelTestComponent
            ],
            imports: [
                NoopAnimationsModule,
                SlbExpandablePanelModule,
            ]
        })
            .compileComponents();
    }));

    it('should toggle its expanded state', () => {
        const fixture = TestBed.createComponent(ExpandablePanelComponent);
        const component = fixture.componentInstance;

        expect(component.expanded).toBe(false);

        component.toggle();
        expect(component.expanded).toBe(true);

        component.toggle();
        expect(component.expanded).toBe(false);
    });

    it('should create', () => {
        const fixture = TestBed.createComponent(PanelTestComponent);
        const component = fixture.componentInstance;
        expect(component).toBeTruthy();
    });

    it('should support two way binding', () => {
        const fixture = TestBed.createComponent(PanelTestComponent);
        const component = fixture.componentInstance;

        expect(component.expanded).toBe(false);

        component.component.toggle();
        expect(component.expanded).toBe(true);

        component.component.toggle();
        expect(component.expanded).toBe(false);

        component.expanded = true;
        fixture.detectChanges();
        expect(component.component.expanded).toBe(true);
    });

    // TODO: write tests for things like clicking the header
});
