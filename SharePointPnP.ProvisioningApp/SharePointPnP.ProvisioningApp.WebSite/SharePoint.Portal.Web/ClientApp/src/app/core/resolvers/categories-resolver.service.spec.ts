import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { RouterStateSnapshot, ActivatedRouteSnapshot, Router } from '@angular/router';
import { throwError, of } from 'rxjs';

import { CategoriesService } from '../api/services';
import { CategoriesResolver } from './categories-resolver.service';
import { Category } from '../api/models';

describe('CategoriesResolverService', () => {
    let service: CategoriesResolver;

    let activatedRouteSnapshotSpy: jasmine.SpyObj<ActivatedRouteSnapshot>;
    let routerStateSnapshotSpy: jasmine.SpyObj<RouterStateSnapshot>;
    let categoriesServiceSpy: jasmine.SpyObj<CategoriesService>;

    beforeEach(() => {
        categoriesServiceSpy = jasmine.createSpyObj('CategoriesService', ['getAll']);
        activatedRouteSnapshotSpy = jasmine.createSpyObj('ActivatedRouteSnapshot', ['']);
        routerStateSnapshotSpy = jasmine.createSpyObj('RouterStateSnapshot', ['']);

        TestBed.configureTestingModule({
            imports: [
                RouterTestingModule
            ],
            providers: [
                { provide: CategoriesService, useValue: categoriesServiceSpy }
            ]
        });

        service = TestBed.get(CategoriesResolver);
    });

    it('should be created', () => expect(service).toBeTruthy());

    it('should return observable with error message if getting categories gives error', () => {
        const message = 'Error';
        categoriesServiceSpy.getAll.and.returnValue(throwError({ message }));

        service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy)
            .subscribe(resolvedCategories =>
                expect(resolvedCategories.errorMessage).toBe(message));
    });

    it('should navigate if the option to redirect on error is given', async () => {
        const router = TestBed.get(Router);
        const navigateSpy = spyOn(router, 'navigate');
        activatedRouteSnapshotSpy.data = {
            categoriesResolverOptions: {
                redirectOnError: { commands: ['/'] }
            }
        };
        const message = 'Error';
        categoriesServiceSpy.getAll.and.returnValue(throwError({ message }));

        await service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy).toPromise();

        expect(navigateSpy).toHaveBeenCalled();
    });

    it('should not navigate if the option to redirect on error is not given', async () => {
        const router = TestBed.get(Router);
        const navigateSpy = spyOn(router, 'navigate');
        const message = 'Error';
        categoriesServiceSpy.getAll.and.returnValue(throwError({ message }));

        await service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy).toPromise();

        expect(navigateSpy).not.toHaveBeenCalled();
    });

    it('should return observable with the categories', () => {
        const testCategories = [new Category()];
        categoriesServiceSpy.getAll.and.returnValue(of(testCategories));

        service.resolve(activatedRouteSnapshotSpy, routerStateSnapshotSpy)
            .subscribe(resolvedCategories =>
                expect(resolvedCategories.data).toBe(testCategories));
    });
});
