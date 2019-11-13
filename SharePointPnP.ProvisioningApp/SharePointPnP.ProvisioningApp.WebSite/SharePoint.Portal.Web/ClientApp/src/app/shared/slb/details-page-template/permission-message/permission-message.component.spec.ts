import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';

import { DetailsPageTemplateData, DetailsPageTemplateDataService } from '../details-page-template-data.service';
import { PermissionMessageComponent } from './permission-message.component';

describe('PermissionMessageComponent', () => {
    let component: PermissionMessageComponent;
    let fixture: ComponentFixture<PermissionMessageComponent>;

    let dataSubject: Subject<DetailsPageTemplateData>;

    beforeEach(async(() => {
        dataSubject = new Subject<DetailsPageTemplateData>();
        const dataServiceMock = {
            data: dataSubject
        };

        TestBed.configureTestingModule({
            declarations: [PermissionMessageComponent],
            providers: [
                { provide: DetailsPageTemplateDataService, useValue: dataServiceMock },
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(PermissionMessageComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => expect(component).toBeTruthy());
});
