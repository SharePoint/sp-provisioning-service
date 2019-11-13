import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { RouterStateSnapshot, ActivatedRouteSnapshot, Router } from '@angular/router';
import { throwError, of } from 'rxjs';

import { Package } from '../api/models';
import { PackagesService } from '../api/services';
import { PackageResolver } from './package-resolver.service';

const testGuid = '0C827DDB-18E9-4709-896B-12B0C34C8E40';

describe('PackageResolverService', () => {
    let service: PackageResolver;

    let activatedRouteSnapshotSpy: jasmine.SpyObj<ActivatedRouteSnapshot>;
    let routerStateSnapshotSpy: jasmine.SpyObj<RouterStateSnapshot>;
    let packagesServiceSpy: jasmine.SpyObj<PackagesService>;

    beforeEach(() => {
        packagesServiceSpy = jasmine.createSpyObj('PackagesService', ['getPackageById']);

        activatedRouteSnapshotSpy = jasmine.createSpyObj('ActivatedRouteSnapshot', ['']);
        activatedRouteSnapshotSpy.params = {};

        routerStateSnapshotSpy = jasmine.createSpyObj('RouterStateNapshot', ['']);

        TestBed.configureTestingModule({
            imports: [
                RouterTestingModule
            ],
            providers: [
                { provide: PackagesService, useValue: packagesServiceSpy }
            ]
        });

        service = TestBed.get(PackageResolver);
    });

    it('should be created', () => expect(service).toBeTruthy());

    it('should return observable with error message if id provided is invalid', () => {
        service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy)
            .subscribe(resolvedPackage =>
                expect(resolvedPackage.errorMessage).toContain('Invalid id'));
    });

    it('should navigate on invalid id if option to redirect on error is given', async () => {
        const router = TestBed.get(Router);
        const navigateSpy = spyOn(router, 'navigate');
        activatedRouteSnapshotSpy.data = {
            packageResolverOptions: {
                redirectOnError: { commands: ['/'] }
            }
        };

        await service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy).toPromise();

        expect(navigateSpy).toHaveBeenCalled();
    });

    it('should return observable with error message if getting package gives error', () => {
        const message = 'Error';
        packagesServiceSpy.getPackageById.and.returnValue(throwError({ message }));
        activatedRouteSnapshotSpy.params.id = testGuid;

        service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy)
            .subscribe(resolvedCategories =>
                expect(resolvedCategories.errorMessage).toBe(message));
    });

    it('should navigate if the option to redirect on error is given', async () => {
        const router = TestBed.get(Router);
        const navigateSpy = spyOn(router, 'navigate');
        activatedRouteSnapshotSpy.params.id = testGuid;
        activatedRouteSnapshotSpy.data = {
            packageResolverOptions: {
                redirectOnError: { commands: ['/'] }
            }
        };
        const message = 'Error';
        packagesServiceSpy.getPackageById.and.returnValue(throwError({ message }));

        await service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy).toPromise();

        expect(navigateSpy).toHaveBeenCalled();
    });

    it('should not navigate if the option to redirect on error is not given', async () => {
        const router = TestBed.get(Router);
        const navigateSpy = spyOn(router, 'navigate');
        const message = 'Error';
        packagesServiceSpy.getPackageById.and.returnValue(throwError({ message }));

        await service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy).toPromise();

        expect(navigateSpy).not.toHaveBeenCalled();
    });

    it('should return observable with the package card', () => {
        const testPackage = new Package();
        activatedRouteSnapshotSpy.params.id = testGuid;
        packagesServiceSpy.getPackageById.and.returnValue(of(testPackage));

        service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy)
            .subscribe(resolvedPackage =>
                expect(resolvedPackage.data).toBe(testPackage));
    });
});
