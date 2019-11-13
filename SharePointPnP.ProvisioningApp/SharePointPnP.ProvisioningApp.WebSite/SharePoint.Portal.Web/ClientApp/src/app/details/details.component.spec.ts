import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ActivatedRoute } from '@angular/router';
import { Title, By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatSnackBar, MatDialog } from '@angular/material';
import { Subject, of, throwError } from 'rxjs';

import { ApplicationSettings, PageTemplate, Package, DisplayInfo } from '../core/api/models';
import { ApplicationSettingsService, PageTemplatesService } from '../core/api/services';
import { ResolvedPackage } from '../core/resolvers/package-resolver.service';
import { DynamicTemplateModule } from '../shared/dynamic-template/dynamic-template.module';
import { SlbDetailsPageTemplateModule } from '../shared/slb/details-page-template/details-page-template.module';
import { SlbExpandablePanelModule } from '../shared/slb/expandable-panel/expandable-panel.module';

import { SlbTabsModule } from '../shared/slb/tabs/tabs.module';
import { SlbStickyHeaderModule } from '../shared/slb/sticky-header/sticky-header.module';

import { DetailsComponent } from './details.component';

describe('DetailsComponent', () => {
    let component: DetailsComponent;
    let fixture: ComponentFixture<DetailsComponent>;

    let activatedRouteParams: Subject<void>;
    let activatedRouteData: Subject<{ resolvedPackage: ResolvedPackage }>;
    let matSnackBarSpy: jasmine.SpyObj<MatSnackBar>;
    let matDialogSpy: jasmine.SpyObj<MatDialog>;
    let titleSpy: jasmine.SpyObj<Title>;
    let pageTemplateServiceSpy: jasmine.SpyObj<PageTemplatesService>;

    beforeEach(async(() => {
        activatedRouteParams = new Subject<void>();
        activatedRouteData = new Subject<{ resolvedPackage: ResolvedPackage }>();
        matSnackBarSpy = jasmine.createSpyObj('MatSnackbar', ['open']);
        matDialogSpy = jasmine.createSpyObj('MatDialog', ['open']);
        titleSpy = jasmine.createSpyObj('Title', ['setTitle']);
        pageTemplateServiceSpy = jasmine.createSpyObj('PageTemplateService', ['getPageTemplate']);

        const activatedRouteSpy = jasmine.createSpyObj('ActivatedRoute', ['']);
        activatedRouteSpy.params = activatedRouteParams;
        activatedRouteSpy.data = activatedRouteData;

        const applicationSettingsSpy: jasmine.SpyObj<ApplicationSettingsService> =
            jasmine.createSpyObj('ApplicationSettingsService', ['getSettings']);
        applicationSettingsSpy.getSettings.and.returnValue(of(new ApplicationSettings()));

        TestBed.configureTestingModule({
            imports: [
                RouterTestingModule,
                NoopAnimationsModule,
                DynamicTemplateModule,
                SlbExpandablePanelModule,
                SlbTabsModule,
                SlbStickyHeaderModule,
                SlbDetailsPageTemplateModule,
            ],
            declarations: [
                DetailsComponent
            ],
            providers: [
                { provide: ApplicationSettingsService, useValue: applicationSettingsSpy },
                { provide: ActivatedRoute, useValue: activatedRouteSpy },
                { provide: MatDialog, useValue: matDialogSpy },
                { provide: MatSnackBar, useValue: matSnackBarSpy },
                { provide: Title, useValue: titleSpy },
                { provide: PageTemplatesService, useValue: pageTemplateServiceSpy },
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(DetailsComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    describe('simple tests', () => {
        let fakeTemplateId;

        beforeEach(() => {
            fakeTemplateId = 'templateguid';
            const packageData = new Package();
            packageData.displayInfo = new DisplayInfo();
            packageData.displayInfo.pageTemplateId = fakeTemplateId;

            pageTemplateServiceSpy.getPageTemplate.and.returnValue(of(new PageTemplate()));

            activatedRouteParams.next();
            activatedRouteData.next({ resolvedPackage: { data: packageData } });
        });

        it('should create', () => expect(component).toBeTruthy());

        it('should not display any error message', () => {
            expect(matSnackBarSpy.open).not.toHaveBeenCalled();
        });

        it('should set the title', () => expect(titleSpy.setTitle).toHaveBeenCalled());

        it('should get the page template', () => {
            expect(pageTemplateServiceSpy.getPageTemplate).toHaveBeenCalledWith(fakeTemplateId);
        });
    });

    describe('resolved data tests', () => {
        beforeEach(() => {
            activatedRouteParams.next();
            pageTemplateServiceSpy.getPageTemplate.and.returnValue(of(new PageTemplate()));
        });

        it('should give an error message if the route data has an error', () => {
            const errorMessage = 'Error occurred';
            activatedRouteData.next({ resolvedPackage: { errorMessage } });

            expect(matSnackBarSpy.open).toHaveBeenCalledTimes(1);
            expect(matSnackBarSpy.open).toHaveBeenCalledWith(errorMessage, 'Dismiss');
        });

        it('should set the data properly', () => {
            const data = new Package();
            data.displayName = 'My Title';
            data.displayInfo = new DisplayInfo();
            data.displayInfo.pageTemplateId = 'templateid';
            activatedRouteData.next({ resolvedPackage: { data } });

            expect(component.detailsTemplateDefinition.context).toEqual(jasmine.objectContaining({
                packageTitle: data.displayName
            }));
        });
    });

    describe('error retrieving page template', () => {
        const message = 'Error message';

        beforeEach(() => {
            pageTemplateServiceSpy.getPageTemplate.and.returnValue(throwError({ message }));
            const packageData = new Package();
            packageData.displayInfo = new DisplayInfo();
            packageData.displayInfo.pageTemplateId = 'templateId';

            activatedRouteParams.next();
            activatedRouteData.next({ resolvedPackage: { data: packageData } });
            fixture.detectChanges();
        });

        it('should use the default template if there is an error retrieving the template', () => {
            expect(matSnackBarSpy.open).not.toHaveBeenCalled();
            const pageElem = fixture.debugElement.query(By.css('.page-wrapper'));
            expect(pageElem).toBeTruthy();
        });
    });
});
