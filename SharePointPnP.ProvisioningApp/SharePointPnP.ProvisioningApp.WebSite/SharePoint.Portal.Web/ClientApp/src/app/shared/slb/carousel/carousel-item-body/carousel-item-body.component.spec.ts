import { Component, TemplateRef, AfterContentInit, ViewContainerRef, ViewChild } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { PortalModule, TemplatePortal } from '@angular/cdk/portal';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { CarouselItemBodyComponent } from './carousel-item-body.component';

@Component({
    template: `
        <ng-template>Body Content</ng-template>
        <slb-carousel-item-body [content]="content" [isActive]="isActive" [direction]="direction"></slb-carousel-item-body>
    `
})
class SimpleCarouselItemBodyTestComponent implements AfterContentInit {
    content: TemplatePortal;
    isActive: boolean;
    direction: 'left' | 'right';

    /**
     * Reference to item body component
     */
    @ViewChild(CarouselItemBodyComponent, { static: false }) itemBody: CarouselItemBodyComponent;
    @ViewChild(TemplateRef, { static: true }) template: TemplateRef<any>;

    constructor(private viewContainerRef: ViewContainerRef) { }

    ngAfterContentInit() {
        this.content = new TemplatePortal(this.template, this.viewContainerRef);
    }
}

describe('CarouselItemBodyComponent', () => {
    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [
                PortalModule,
                NoopAnimationsModule
            ],
            declarations: [
                CarouselItemBodyComponent,
                SimpleCarouselItemBodyTestComponent
            ]
        })
            .compileComponents();
    }));

    describe('simple tests', () => {
        let component: CarouselItemBodyComponent;
        let fixture: ComponentFixture<CarouselItemBodyComponent>;

        beforeEach(() => {
            fixture = TestBed.createComponent(CarouselItemBodyComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should create', () => {
            expect(component).toBeTruthy();
        });
    });

    describe('should properly set the animation state', () => {
        let component: SimpleCarouselItemBodyTestComponent;
        let fixture: ComponentFixture<SimpleCarouselItemBodyTestComponent>;

        beforeEach(() => {
            fixture = TestBed.createComponent(SimpleCarouselItemBodyTestComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
        });

        it('should set it to center', () => {
            component.isActive = true;
            fixture.detectChanges();
            expect(component.itemBody.animationState).toEqual('center');
        });

        it('should set to correct state when active and going left', () => {
            component.isActive = true;
            component.direction = 'left';
            fixture.detectChanges();
            expect(component.itemBody.animationState).toEqual('leftToCenter');
        });

        it('should set to correct state when active and going right', () => {
            component.isActive = true;
            component.direction = 'right';
            fixture.detectChanges();
            expect(component.itemBody.animationState).toEqual('rightToCenter');
        });

        it('should set it to left when going right', () => {
            component.isActive = false;
            component.direction = 'right';
            fixture.detectChanges();
            expect(component.itemBody.animationState).toEqual('left');
        });

        it('should set it to right when going left', () => {
            component.isActive = false;
            component.direction = 'left';
            fixture.detectChanges();
            expect(component.itemBody.animationState).toEqual('right');
        });
    });
});
