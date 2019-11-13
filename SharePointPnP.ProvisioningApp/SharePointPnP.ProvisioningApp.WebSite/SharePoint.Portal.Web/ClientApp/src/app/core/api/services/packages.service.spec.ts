import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { Package } from '../models';
import { PackagesService } from './packages.service';

const testPackageData: Package = new Package();
testPackageData.id = 'fakeGuid';
testPackageData.displayName = 'Test Template Card';
testPackageData.description = 'This template is a test';

describe('PackagessService', () => {
    let httpMock: HttpTestingController;
    let service: PackagesService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule]
        });
        httpMock = TestBed.get(HttpTestingController);
        service = TestBed.get(PackagesService);
    });

    afterEach(() => httpMock.verify());

    it('should be created', () => expect(service).toBeTruthy());

    it('shoud get the template cards', () => {
        const expectedPackages: Package[] = [testPackageData];

        service.getPackages().subscribe(cards => {
            expect(cards.length).toBe(1);
            expect(cards).toEqual(expectedPackages);
        });

        const request = httpMock.expectOne('/api/packages');
        expect(request.request.method).toEqual('GET');
        request.flush(expectedPackages);
    });

    it('should get template card by id', () => {
        const expectedTemplateCard = testPackageData;

        service.getPackageById('guidgoeshere').subscribe(card => {
            expect(card).toEqual(expectedTemplateCard);
        });

        const request = httpMock.expectOne('/api/packages/guidgoeshere');
        expect(request.request.method).toEqual('GET');
        request.flush(expectedTemplateCard);
    });
});
