import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';

import { DetailsPageTemplateData, DetailsPageTemplateDataService } from '../details-page-template-data.service';

import { SiteDescriptorComponent } from './site-descriptor.component';

describe('SiteDescriptorComponent', () => {
    let component: SiteDescriptorComponent;
    let fixture: ComponentFixture<SiteDescriptorComponent>;

    let dataSubject: Subject<DetailsPageTemplateData>;

    beforeEach(async(() => {
        dataSubject = new Subject<DetailsPageTemplateData>();
        const dataServiceMock = {
            data: dataSubject
        };

        TestBed.configureTestingModule({
            declarations: [SiteDescriptorComponent],
            providers: [
                { provide: DetailsPageTemplateDataService, useValue: dataServiceMock },
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(SiteDescriptorComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => expect(component).toBeTruthy());

    it('should display the descriptor', () => {
        const data = new DetailsPageTemplateData();
        data.packageDescriptor = 'Test';
        dataSubject.next(data);
        fixture.detectChanges();

        expect(fixture.debugElement.nativeElement.innerText).toEqual('Test');
    });
});
