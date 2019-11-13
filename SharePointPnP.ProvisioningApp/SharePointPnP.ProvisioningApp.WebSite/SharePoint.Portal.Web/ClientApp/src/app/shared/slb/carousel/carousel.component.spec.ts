import { Component, ViewChild } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { SlbCarouselModule } from './carousel.module';
import { By } from '@angular/platform-browser';
import { CarouselComponent } from './carousel.component';

@Component({
    template: `
    <slb-carousel (selectedIndexChange)="selectedIndex = $event" #carousel>
        <slb-carousel-item></slb-carousel-item>
    </slb-carousel>
    `
})
class SingleItemCarouselTestComponent {
    selectedIndex: number;
    @ViewChild('carousel', { static: true }) carousel: CarouselComponent;
}

@Component({
    template: `
    <slb-carousel (selectedIndexChange)="selectedIndex = $event" #carousel>
        <slb-carousel-item></slb-carousel-item>
        <slb-carousel-item></slb-carousel-item>
    </slb-carousel>
    `
})
class TwoItemCarouselTestComponent {
    @ViewChild('carousel', { static: true }) carousel: CarouselComponent;
    selectedIndex: number;
}

@Component({
    template: `
    <slb-carousel (selectedIndexChange)="selectedIndex = $event">
        <slb-carousel-item></slb-carousel-item>
        <slb-carousel-item></slb-carousel-item>
        <slb-carousel-item></slb-carousel-item>
    </slb-carousel>
    `
})
class ThreeItemCarouselTestComponent {
    selectedIndex: number;
}

@Component({
    template: `
    <slb-carousel (selectedIndexChange)="selectedIndex = $event" #carousel>
        <slb-carousel-item *ngFor="let item of items"></slb-carousel-item>
    </slb-carousel>
    `
})
class DynamicItemCarouselTestComponent {
    @ViewChild('carousel', { static: true }) carousel: CarouselComponent;
    selectedIndex: number;
    items: any[] = [];
}

describe('CarouselComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [SlbCarouselModule, NoopAnimationsModule],
            declarations: [
                SingleItemCarouselTestComponent,
                TwoItemCarouselTestComponent,
                ThreeItemCarouselTestComponent,
                DynamicItemCarouselTestComponent
            ]
        })
            .compileComponents();
    }));

    describe('simple tests', () => {
        let fixture: ComponentFixture<CarouselComponent>;
        let component: CarouselComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(CarouselComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should create', () => expect(component).toBeTruthy());

        it('should default to looping', () => {
            expect(component.doLoopItems).toBe(true);
        });
    });

    describe('single item carousel', () => {
        let fixture: ComponentFixture<SingleItemCarouselTestComponent>;
        let component: SingleItemCarouselTestComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(SingleItemCarouselTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should not show control buttons for single item', () => {
            const buttons = fixture.debugElement.queryAll(By.css('button'));
            expect(buttons.length).toBe(0);
        });

        it('should set the initial selected index to 0', () => {
            expect(component.selectedIndex).toBe(0);
        });

        it('should go nowhere when going left or right on the carousel', () => {
            component.carousel.goLeft();
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(0);

            component.carousel.goLeft();
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(0);

            component.carousel.goRight();
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(0);
        });
    });

    describe('two item carousel', () => {
        let fixture: ComponentFixture<TwoItemCarouselTestComponent>;
        let component: TwoItemCarouselTestComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(TwoItemCarouselTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should set the initial selected index to 0', () => {
            expect(component.selectedIndex).toBe(0);
        });

        it('should cycle through the selected index', () => {
            const leftButton = fixture.debugElement.queryAll(By.css('button'))[0];
            const rightButton = fixture.debugElement.queryAll(By.css('button'))[1];

            leftButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(1, 'looped back to end correctly');

            leftButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(0);

            leftButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(1);

            leftButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(0);

            rightButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(1, 'goes forward from beginning correctly');
        });
    });

    describe('three item carousel', () => {
        let fixture: ComponentFixture<ThreeItemCarouselTestComponent>;
        let component: ThreeItemCarouselTestComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(ThreeItemCarouselTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should set the initial selected index to 0', () => {
            expect(component.selectedIndex).toBe(0);
        });

        it('should cycle through the selected index', () => {
            const leftButton = fixture.debugElement.queryAll(By.css('button'))[0];
            const rightButton = fixture.debugElement.queryAll(By.css('button'))[1];

            leftButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(2);

            leftButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(1);

            leftButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(0);

            leftButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(2);

            rightButton.triggerEventHandler('click', {});
            fixture.detectChanges();
            expect(component.selectedIndex).toBe(0);
        });
    });

    describe('dynamic items', () => {
        let fixture: ComponentFixture<DynamicItemCarouselTestComponent>;
        let component: DynamicItemCarouselTestComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(DynamicItemCarouselTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should handle adding an item when there were no items', () => {
            component.items.push({});
            fixture.detectChanges();

            expect(component.selectedIndex).toBe(0, 'selects first item');
            expect(component.carousel.items.toArray()[0].isActive).toBe(true, 'first item correctly active');
        });

        it('should handle replacing the items', () => {
            component.items = [{}, {}, {}];
            fixture.detectChanges();

            let carouselItems = component.carousel.items.toArray();
            expect(component.selectedIndex).toBe(0, 'selects first item');
            expect(carouselItems[0].isActive).toBe(true, 'first item correctly active');
            expect(carouselItems[1].isActive).toBe(false, 'second item correctly inactive');
            expect(carouselItems[2].isActive).toBe(false, 'third item correctly inactive');

            component.carousel.goRight();
            fixture.detectChanges();

            component.items = [{}, {}, {}];
            fixture.detectChanges();

            carouselItems = component.carousel.items.toArray();
            expect(component.selectedIndex).toBe(0, 'selects first item after replacement with 3 new items');
            expect(carouselItems[0].isActive).toBe(true, 'first item correctly active after 3 item replacement');
            expect(carouselItems[1].isActive).toBe(false, 'second item correctly inactive after 3 item replacement');
            expect(carouselItems[2].isActive).toBe(false, 'third item correctly inactive after 3 item replacement');

            component.carousel.goRight();
            fixture.detectChanges();

            component.items = [{}, {}];
            fixture.detectChanges();

            carouselItems = component.carousel.items.toArray();
            expect(component.selectedIndex).toBe(0, 'selects first item after replacement with 2 new items');
            expect(carouselItems[0].isActive).toBe(true, 'first item correctly active after 2 item replacement');
            expect(carouselItems[1].isActive).toBe(false, 'second item correctly active after 2 item replacement');
        });

        it('should handle inserting items at the beginning or in the middle', () => {
            component.items = [{}, {}, {}];
            fixture.detectChanges();

            component.items.unshift({});
            fixture.detectChanges();

            let carouselItems = component.carousel.items.toArray();
            expect(component.selectedIndex).toBe(1, 'keeps previously selected item after replacement');
            expect(carouselItems[0].isActive).toBe(false, 'new first item correctly inactive');
            expect(carouselItems[1].isActive).toBe(true, 'previous item still active');
            expect(carouselItems[2].isActive).toBe(false);
            expect(carouselItems[3].isActive).toBe(false);

            component.items.splice(1, 0, {}, {});
            fixture.detectChanges();

            carouselItems = component.carousel.items.toArray();
            expect(component.selectedIndex).toBe(3, 'keeps previously selected item');
            expect(carouselItems[1].isActive).toBe(false);
            expect(carouselItems[2].isActive).toBe(false);
            expect(carouselItems[3].isActive).toBe(true, 'item still active');
            expect(carouselItems[4].isActive).toBe(false);
        });

        it('should handle removing items', () => {
            component.items = [{}, {}, {}, {}, {}];
            fixture.detectChanges();
            component.carousel.goRight();
            fixture.detectChanges();
            component.carousel.goRight();
            fixture.detectChanges();
            component.carousel.goRight();
            fixture.detectChanges();

            expect(component.selectedIndex).toBe(3, 'expected index is selected');

            component.items.shift();
            fixture.detectChanges();

            let carouselItems = component.carousel.items.toArray();
            expect(component.selectedIndex).toBe(2, 'keeps previously selected item after replacement');
            expect(carouselItems[1].isActive).toBe(false);
            expect(carouselItems[2].isActive).toBe(true);
            expect(carouselItems[3].isActive).toBe(false);

            component.items.splice(1, 1);
            fixture.detectChanges();

            carouselItems = component.carousel.items.toArray();
            expect(component.selectedIndex).toBe(1, 'keeps previously selected item');
            expect(carouselItems[0].isActive).toBe(false);
            expect(carouselItems[1].isActive).toBe(true);
            expect(carouselItems[2].isActive).toBe(false);
        });

        it('should show the available control buttons', () => {
            component.items = [{}, {}];
            component.carousel.doLoopItems = false;
            fixture.detectChanges();

            let buttons = fixture.debugElement.queryAll(By.css('button'));
            expect(buttons.length).toBe(1, 'only one button should be visible');
            let button = buttons[0].nativeElement as HTMLElement;
            expect(button.classList.contains('carousel-forward-button')).toBe(true);

            buttons[0].triggerEventHandler('click', {});
            fixture.detectChanges();

            buttons = fixture.debugElement.queryAll(By.css('button'));
            expect(buttons.length).toBe(1);
            button = buttons[0].nativeElement as HTMLElement;
            expect(button.classList.contains('carousel-back-button')).toBe(true);

            component.carousel.doLoopItems = true;
            fixture.detectChanges();
            expect(buttons.length).toBe(1);
        });
    });
});
