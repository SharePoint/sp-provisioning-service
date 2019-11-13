import { async, TestBed, ComponentFixture } from '@angular/core/testing';
import { Component } from '@angular/core';

import { SlbDetailsPageTemplateModule } from './details-page-template.module';
import { DetailsPageTemplateData, DetailsPageTemplateDataService } from './details-page-template-data.service';

@Component({
    template: '<div *slbDetailsPageTemplateContext="context"></div>'
})
class DetailsTemplateContextTestComponent {
    context: DetailsPageTemplateData;
}

describe('DisplayTemplateContextDirective', () => {
    let fixture: ComponentFixture<DetailsTemplateContextTestComponent>;
    let component: DetailsTemplateContextTestComponent;

    let dataServiceSpy: jasmine.SpyObj<DetailsPageTemplateDataService>;

    beforeEach(async(() => {
        dataServiceSpy = jasmine.createSpyObj('DetailsPageTemplateDataService', ['setDetailData']);

        TestBed.configureTestingModule({
            imports: [SlbDetailsPageTemplateModule],
            declarations: [DetailsTemplateContextTestComponent]
        })
            .compileComponents();

        TestBed.overrideProvider(DetailsPageTemplateDataService, { useValue: dataServiceSpy });
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(DetailsTemplateContextTestComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create an instance', () => expect(component).toBeTruthy());

    it('should call to set the data', () => {
        expect(dataServiceSpy.setDetailData).toHaveBeenCalled();
    });

    it('should call to set the data when changed', () => {
        const testContext = new DetailsPageTemplateData();
        dataServiceSpy.setDetailData.calls.reset();

        component.context = testContext;
        fixture.detectChanges();

        expect(dataServiceSpy.setDetailData).toHaveBeenCalledWith(testContext);
    });
});
