import { TestBed } from '@angular/core/testing';
import { HttpTestingController, HttpClientTestingModule } from '@angular/common/http/testing';

import { ApplicationSettingsService } from './application-settings.service';
import { ApplicationSettings } from '../models/application-settings.model';

describe('ApplicationSettingsService', () => {
    let httpMock: HttpTestingController;
    let service: ApplicationSettingsService;

    beforeEach(() => TestBed.configureTestingModule({
        imports: [HttpClientTestingModule]
    }));

    beforeEach(() => {
        httpMock = TestBed.get(HttpTestingController);
        service = TestBed.get(ApplicationSettingsService);
    });

    afterEach(() => httpMock.verify());

    it('should be created', () => expect(service).toBeTruthy());

    it('should get the settings', () => {
        service.getSettings().subscribe();

        const req = httpMock.expectOne('/api/settings');
        expect(req.request.method).toEqual('GET');
        req.flush(new ApplicationSettings());
    });

    it('shoulod only call to get the settings the first time', () => {
        service.getSettings().subscribe();
        service.getSettings().subscribe();
        service.getSettings().subscribe();

        const req = httpMock.expectOne('/api/settings');
        expect(req.request.method).toEqual('GET');
        req.flush(new ApplicationSettings());
    });
});
