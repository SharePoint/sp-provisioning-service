import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';

import { ApplicationSettingsService } from '../api/services/application-settings.service';

import { FooterComponent } from './footer.component';
import { SlbDateFormatModule } from 'src/app/shared/slb/date-format/date-format.module';

describe('FooterComponent', () => {
    let component: FooterComponent;
    let fixture: ComponentFixture<FooterComponent>;

    let appSettingsServiceSpy: jasmine.SpyObj<ApplicationSettingsService>;
    let settingsSubject: Subject<any>;

    beforeEach(async(() => {
        settingsSubject = new Subject();
        appSettingsServiceSpy = jasmine.createSpyObj('ApplicationSettingsService', ['getSettings']);
        appSettingsServiceSpy.getSettings.and.returnValue(settingsSubject);

        TestBed.configureTestingModule({
            imports: [
                SlbDateFormatModule
            ],
            declarations: [FooterComponent],
            providers: [
                { provide: ApplicationSettingsService, useValue: appSettingsServiceSpy }
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(FooterComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should get the date from the server', () => {
        expect(appSettingsServiceSpy.getSettings).toHaveBeenCalledTimes(1);
    });
});
