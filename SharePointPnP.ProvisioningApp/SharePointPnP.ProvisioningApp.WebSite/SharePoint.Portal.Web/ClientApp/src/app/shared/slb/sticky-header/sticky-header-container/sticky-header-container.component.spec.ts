import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StickyHeaderContainerComponent } from './sticky-header-container.component';
import { SlbStickyHeaderModule } from '../sticky-header.module';

describe('StickyHeaderContainerComponent', () => {
    let component: StickyHeaderContainerComponent;
    let fixture: ComponentFixture<StickyHeaderContainerComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [
                SlbStickyHeaderModule
            ],
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(StickyHeaderContainerComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
