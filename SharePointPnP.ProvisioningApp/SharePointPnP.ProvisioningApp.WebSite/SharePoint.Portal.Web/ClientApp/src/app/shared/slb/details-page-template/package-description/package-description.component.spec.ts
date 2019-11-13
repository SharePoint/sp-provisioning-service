import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';

import { DetailsPageTemplateData, DetailsPageTemplateDataService } from '../details-page-template-data.service';

import { PackageDescriptionComponent } from './package-description.component';

describe('PackageDescriptionComponent', () => {
    let component: PackageDescriptionComponent;
    let fixture: ComponentFixture<PackageDescriptionComponent>;

    let dataSubject: Subject<DetailsPageTemplateData>;

    beforeEach(async(() => {
        dataSubject = new Subject<DetailsPageTemplateData>();
        const dataServiceMock = {
            data: dataSubject
        };

        TestBed.configureTestingModule({
            declarations: [PackageDescriptionComponent],
            providers: [
                { provide: DetailsPageTemplateDataService, useValue: dataServiceMock },
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(PackageDescriptionComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => expect(component).toBeTruthy());
});
