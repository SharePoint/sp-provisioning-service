import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicTemplateComponent } from './dynamic-template.component';

describe('DynamicTemplateComponent', () => {
    let component: DynamicTemplateComponent;
    let fixture: ComponentFixture<DynamicTemplateComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [
                DynamicTemplateComponent
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(DynamicTemplateComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
