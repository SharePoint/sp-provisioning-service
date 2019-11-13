import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';

import { PageTemplatesService } from '../api/services';

import { PageTemplateResolver } from './page-template-resolver.service';
import { RouterTestingModule } from '@angular/router/testing';
import { throwError, of } from 'rxjs';
import { PageTemplate } from '../api/models';

const testId = 'this/is/a/test';

describe('PageTemplateResolverService', () => {
    let service: PageTemplateResolver;

    let activatedRouteSnapshotSpy: jasmine.SpyObj<ActivatedRouteSnapshot>;
    let routerStateSnapshotSpy: jasmine.SpyObj<RouterStateSnapshot>;
    let pageTemplateServiceSpy: jasmine.SpyObj<PageTemplatesService>;

    beforeEach(() => {
        pageTemplateServiceSpy = jasmine.createSpyObj('PageTemplateService', ['getPageTemplate']);

        activatedRouteSnapshotSpy = jasmine.createSpyObj('ActivatedRouteSnapshot', ['']);
        activatedRouteSnapshotSpy.params = {};

        routerStateSnapshotSpy = jasmine.createSpyObj('RouterStateSnapshot', ['']);

        TestBed.configureTestingModule({
            imports: [
                RouterTestingModule
            ],
            providers: [
                { provide: PageTemplatesService, useValue: pageTemplateServiceSpy }
            ]
        });

        service = TestBed.get(PageTemplateResolver);
    });

    it('should be created', () => expect(service).toBeTruthy());

    it('should return observable with error message if getting template gives error', () => {
        const router = TestBed.get(Router);
        const navigateSpy = spyOn(router, 'navigate');
        const message = 'Error';
        activatedRouteSnapshotSpy.params.id = testId;
        pageTemplateServiceSpy.getPageTemplate.and.returnValue(throwError({ message }));

        service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy)
            .subscribe(resolvedPageTemplate =>
                expect(resolvedPageTemplate.errorMessage).toBe(message));

        expect(navigateSpy).not.toHaveBeenCalled();
    });

    it('should navigate on error if the option to redirect is given', async () => {
        const router = TestBed.get(Router);
        const navigateSpy = spyOn(router, 'navigate');
        activatedRouteSnapshotSpy.params.id = testId;
        activatedRouteSnapshotSpy.data = {
            pageTemplateResolverOptions: {
                redirectOnError: { commands: ['/'] }
            }
        };
        const message = 'Error';
        pageTemplateServiceSpy.getPageTemplate.and.returnValue(throwError({message}));

        await service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy).toPromise();

        expect(navigateSpy).toHaveBeenCalled();
    });

    it('should return observable with the page template', async () => {
        const testPageTemplate = new PageTemplate();
        activatedRouteSnapshotSpy.params.id = testId;
        pageTemplateServiceSpy.getPageTemplate.and.returnValue(of(testPageTemplate));

        const template = await service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy).toPromise();
        expect(template.data).toBe(testPageTemplate);
    });
});
