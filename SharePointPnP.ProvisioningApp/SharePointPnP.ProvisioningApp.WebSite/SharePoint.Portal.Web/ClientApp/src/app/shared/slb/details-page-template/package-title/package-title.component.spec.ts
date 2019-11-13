import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';

import { DetailsPageTemplateData, DetailsPageTemplateDataService } from '../details-page-template-data.service';

import { PackageTitleComponent } from './package-title.component';

describe('PackageTitleComponent', () => {
    let component: PackageTitleComponent;
    let fixture: ComponentFixture<PackageTitleComponent>;

    let dataSubject: Subject<DetailsPageTemplateData>;

    beforeEach(async(() => {
        dataSubject = new Subject<DetailsPageTemplateData>();
        const dataServiceMock = {
            data: dataSubject
        };

        TestBed.configureTestingModule({
            declarations: [PackageTitleComponent],
            providers: [
                { provide: DetailsPageTemplateDataService, useValue: dataServiceMock },
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(PackageTitleComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => expect(component).toBeTruthy());
});
