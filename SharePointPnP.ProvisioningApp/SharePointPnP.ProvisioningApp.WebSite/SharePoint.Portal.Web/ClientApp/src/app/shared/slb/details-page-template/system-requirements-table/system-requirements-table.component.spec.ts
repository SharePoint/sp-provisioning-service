import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';

import { DetailsPageTemplateData, DetailsPageTemplateDataService } from '../details-page-template-data.service';

import { SystemRequirementsTableComponent } from './system-requirements-table.component';
import { By } from '@angular/platform-browser';
import { SystemRequirement } from 'src/app/core/api/models';

describe('SystemRequirementsTableComponent', () => {
    let component: SystemRequirementsTableComponent;
    let fixture: ComponentFixture<SystemRequirementsTableComponent>;

    let testData: DetailsPageTemplateData;
    let dataSubject: Subject<DetailsPageTemplateData>;

    beforeEach(async(() => {
        testData = new DetailsPageTemplateData();

        dataSubject = new Subject<DetailsPageTemplateData>();
        const dataServiceMock = {
            data: dataSubject
        };

        TestBed.configureTestingModule({
            declarations: [SystemRequirementsTableComponent],
            providers: [
                { provide: DetailsPageTemplateDataService, useValue: dataServiceMock },
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(SystemRequirementsTableComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => expect(component).toBeTruthy());

    it('should not display the no requirements message if the data has not been loaded', () => {
        const noReqMessage = fixture.debugElement.query(By.css('.no-requirements-message'));
        expect(noReqMessage).toBeFalsy();
    });

    it('should display the no requirements message if there are no requirements', () => {
        testData.systemRequirements = null;
        dataSubject.next(testData);
        fixture.detectChanges();
        const noReqMessage = fixture.debugElement.query(By.css('.no-requirements-message'));
        expect(noReqMessage).toBeTruthy();
    });

    it('should display the no requirements message if requirements is empty', () => {
        testData.systemRequirements = [];
        dataSubject.next(testData);
        fixture.detectChanges();
        const noReqMessage = fixture.debugElement.query(By.css('.no-requirements-message'));
        expect(noReqMessage).toBeTruthy();
    });

    it('should display the requirements if there are requirements', () => {
        testData.systemRequirements = [new SystemRequirement(), new SystemRequirement()];
        dataSubject.next(testData);
        fixture.detectChanges();

        const noReqMessage = fixture.debugElement.query(By.css('.no-requirements-message'));
        expect(noReqMessage).toBeFalsy();

        const requirementsTable = fixture.debugElement.query(By.css('table'));
        expect(requirementsTable).toBeTruthy();

        const rows = requirementsTable.queryAll(By.css('tbody tr'));
        expect(rows.length).toBe(2);
    });
});
