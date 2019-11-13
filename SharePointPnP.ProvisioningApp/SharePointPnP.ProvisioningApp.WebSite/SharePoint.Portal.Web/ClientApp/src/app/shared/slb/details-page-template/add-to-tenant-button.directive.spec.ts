import { Component, ViewChild } from '@angular/core';
import { TestBed, async, ComponentFixture } from '@angular/core/testing';
import { of, Subject } from 'rxjs';

import { ApplicationSettingsService } from 'src/app/core/api/services/application-settings.service';
import { DetailsPageTemplateDataService, DetailsPageTemplateData } from './details-page-template-data.service';

import { AddToTenantButtonDirective } from './add-to-tenant-button.directive';

@Component({
    template: '<button slbAddToTenantButton>Add</button>'
})
export class AddToTenantButtonTestComponent {
    @ViewChild(AddToTenantButtonDirective, { static: true }) directive: AddToTenantButtonDirective;
}

describe('AddToTenantButtonDirective', () => {
    let fixture: ComponentFixture<AddToTenantButtonTestComponent>;
    let component: AddToTenantButtonTestComponent;

    let dataSubject: Subject<DetailsPageTemplateData>;

    beforeEach(async(() => {
        dataSubject = new Subject<DetailsPageTemplateData>();
        const dataServiceMock = {
            data: dataSubject
        };

        TestBed.configureTestingModule({
            declarations: [
                AddToTenantButtonTestComponent,
                AddToTenantButtonDirective
            ],
            providers: [
                { provide: DetailsPageTemplateDataService, useValue: dataServiceMock }
            ]
        });
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(AddToTenantButtonTestComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create an instance', () => expect(component).toBeTruthy());

    it('should have the correct provisioning url', () => {
        const data = new DetailsPageTemplateData();

        data.provisioningFormUrl = 'http://provisioning-form-url';
        dataSubject.next(data);
        expect(component.directive.provisioningFormUrl.startsWith('http://provisioning-form-url?returnUrl=http://'))
            .toBe(true, 'adds new query string');

        data.provisioningFormUrl = 'http://provisioning-form-url?packageId=1234';
        dataSubject.next(data);
        expect(component.directive.provisioningFormUrl.startsWith('http://provisioning-form-url?packageId=1234&returnUrl=http://'))
            .toBe(true, 'adds another query parameter');
    });
});
