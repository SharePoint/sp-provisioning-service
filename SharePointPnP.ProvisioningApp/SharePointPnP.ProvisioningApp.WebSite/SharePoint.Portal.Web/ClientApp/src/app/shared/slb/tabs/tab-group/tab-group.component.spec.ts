import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, ViewChild } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { SlbTabsModule } from '../tabs.module';
import { TabGroupComponent } from './tab-group.component';
import { By } from '@angular/platform-browser';
import { LEFT_ARROW } from '@angular/cdk/keycodes';

@Component({
    template: `
    <slb-tab-group (selectedIndexChange)="selectedIndex = $event" #tabgroup>
        <slb-tab label="A Label">Tab Contents</slb-tab>
    </slb-tab-group>
    `
})
class SingleTabTestComponent {
    @ViewChild('tabgroup', { static: true }) tabGroup: TabGroupComponent;
    selectedIndex: number;
}

@Component({
    template: `
    <slb-tab-group (selectedIndexChange)="selectedIndex = $event" #tabgroup>
        <slb-tab label="A Label">Tab Contents</slb-tab>
        <slb-tab label="A second Label">Tab Contents</slb-tab>
        <slb-tab label="A third Label">Tab Contents</slb-tab>
    </slb-tab-group>
    `
})
class TripleTabTestComponent {
    @ViewChild('tabgroup', { static: true }) tabGroup: TabGroupComponent;
    selectedIndex: number;
}

@Component({
    template: `
    <slb-tab-group (selectedIndexChange)="selectedIndex = $event" #tabgroup>
        <slb-tab *ngFor="let label of labels" [label]="label"></slb-tab>
    </slb-tab-group>
    `
})
class DynamicTabTestComponent {
    @ViewChild('tabgroup', { static: true }) tabGroup: TabGroupComponent;
    labels: string[];
    selectedIndex: number;
}

describe('TabGroupComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [
                SlbTabsModule,
                NoopAnimationsModule
            ],
            declarations: [
                SingleTabTestComponent,
                TripleTabTestComponent,
                DynamicTabTestComponent
            ]
        })
            .compileComponents();
    }));

    describe('simple tests', () => {
        let fixture: ComponentFixture<TabGroupComponent>;
        let component: TabGroupComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(TabGroupComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should create', () => expect(component).toBeTruthy());
    });

    describe('single tab', () => {
        let fixture: ComponentFixture<SingleTabTestComponent>;
        let component: SingleTabTestComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(SingleTabTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should set the initial selected index to 0', () => expect(component.selectedIndex).toBe(0));
    });

    describe('keyboard navigation', () => {
        let fixture: ComponentFixture<TripleTabTestComponent>;
        let component: TripleTabTestComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(TripleTabTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should change focus when the end key is used to navigate', () => {
            const tablabelContainer = fixture.debugElement.query(By.css('.slb-tab-label-wrapper')).nativeElement as HTMLElement;
            const tabLabels = fixture.debugElement.queryAll(By.css('slb-tab-label'));

            // Make sure we are focused in the tab labels
            tabLabels[0].nativeElement.click();
            fixture.detectChanges();

            expect(component.selectedIndex).toBe(0);

            const labelSpy = spyOn(tabLabels[2].componentInstance, 'focus');
            tablabelContainer.dispatchEvent(new KeyboardEvent('keydown', { key: 'End' }));
            expect(labelSpy).toHaveBeenCalled();
        });

        it('should change focus when the home key is used to navigate', () => {
            const tablabelContainer = fixture.debugElement.query(By.css('.slb-tab-label-wrapper')).nativeElement as HTMLElement;
            const tabLabels = fixture.debugElement.queryAll(By.css('slb-tab-label'));

            // Make sure we are focused in the tab labels
            tabLabels[2].nativeElement.click();
            fixture.detectChanges();

            expect(component.selectedIndex).toBe(2);

            const labelSpy = spyOn(tabLabels[0].componentInstance, 'focus');
            tablabelContainer.dispatchEvent(new KeyboardEvent('keydown', { key: 'Home' }));
            expect(labelSpy).toHaveBeenCalled();
        });
    });

    describe('dynamic tabs', () => {
        let fixture: ComponentFixture<DynamicTabTestComponent>;
        let component: DynamicTabTestComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(DynamicTabTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should select index of -1 if there are no tabs', () => {
            expect(component.selectedIndex).toBe(-1);
        });

        it('should add a new set tabs', () => {
            component.labels = ['one', 'two'];
            fixture.detectChanges();

            expect(component.selectedIndex).toBe(0);

            component.tabGroup.activateTab(1);
            fixture.detectChanges();

            expect(component.selectedIndex).toBe(1);
        });

        it('should keep the same tab selected if new tabs are added/removed', () => {
            component.labels = ['one', 'two'];
            fixture.detectChanges();
            component.tabGroup.activateTab(1);
            fixture.detectChanges();

            component.labels.push('three');
            fixture.detectChanges();

            expect(component.selectedIndex).toBe(1, 'keeps same selected index when tab added to end');

            component.labels.unshift('zero');
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(2, 'should shift the selected index by one when tab added to start');

            component.labels.splice(1, 1);
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(1, 'should shif the selected index by 1 when tab removed before it');
        });
    });
});
