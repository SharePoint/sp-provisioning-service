import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { PageTemplatesService } from './page-templates.service';
import { PageTemplate } from '../models';

describe('PageTemplatesService', () => {
    let httpMock: HttpTestingController;
    let service: PageTemplatesService;

    beforeEach(() => TestBed.configureTestingModule({
        imports: [HttpClientTestingModule]
    }));

    beforeEach(() => {
        httpMock = TestBed.get(HttpTestingController);
        service = TestBed.get(PageTemplatesService);
    });

    afterEach(() => httpMock.verify());

    it('should be created', () => expect(service).toBeTruthy());

    it('should get the page template', () => {
        service.getPageTemplate('asdfasdf').subscribe();

        const req = httpMock.expectOne('/api/pagetemplates?templateId=asdfasdf');
        expect(req.request.method).toEqual('GET');
        req.flush(new PageTemplate());
    });

    it('should cache the page template and only do the http request once', () => {
        service.getPageTemplate('asdfasdf').subscribe();
        service.getPageTemplate('asdfasdf').subscribe();
        service.getPageTemplate('asdfasdf').subscribe();

        const req = httpMock.expectOne('/api/pagetemplates?templateId=asdfasdf');
        expect(req.request.method).toEqual('GET');
        req.flush(new PageTemplate());
    });
});
