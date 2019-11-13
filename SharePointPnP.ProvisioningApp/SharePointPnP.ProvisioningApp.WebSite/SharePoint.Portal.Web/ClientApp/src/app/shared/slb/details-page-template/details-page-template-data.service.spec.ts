import { TestBed } from '@angular/core/testing';

import { DetailsPageTemplateDataService, DetailsPageTemplateData } from './details-page-template-data.service';

const testData: DetailsPageTemplateData = {
    packageId: null,
    detailItemCategories: null,
    previewImages: null,
    packageDescription: null,
    packageTitle: null,
    packageDescriptor: null,
    packageType: null,
    systemRequirements: null,
    provisioningFormUrl: null,
};

describe('TemplateDataService', () => {
    let service: DetailsPageTemplateDataService;

    beforeEach(() => TestBed.configureTestingModule({
        providers: [DetailsPageTemplateDataService]
    }));

    beforeEach(() => {
        service = TestBed.get(DetailsPageTemplateDataService);
    });

    it('should be created', () => expect(service).toBeTruthy());

    it('should have the snapshot data as the data that is last set', () => {
        service.setDetailData(testData);

        expect(service.dataSnapshot).toBe(testData);
    });

    it('should emit when new data is set', () => {
        const spyFunc = jasmine.createSpy('spy');

        service.data.subscribe(data => spyFunc(data));
        service.setDetailData(null);

        expect(spyFunc).toHaveBeenCalledWith(null);

        spyFunc.calls.reset();
        service.setDetailData(testData);

        expect(spyFunc).toHaveBeenCalledWith(testData);
    });
});
