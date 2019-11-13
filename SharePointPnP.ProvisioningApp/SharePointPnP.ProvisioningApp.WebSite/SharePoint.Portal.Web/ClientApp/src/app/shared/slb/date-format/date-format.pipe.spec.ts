import { Component } from '@angular/core';
import { TestBed, async, ComponentFixture } from '@angular/core/testing';

import { SlbDateFormatModule } from './date-format.module';
import { DEFAULT_DATE_FORMAT } from './default-date-format.token';

import { DateFormatPipe } from './date-format.pipe';

@Component({
    template: `
    {{ date | slbDateFormat }}
    `
})
class DateFormatTestComponent {
    date: string;
}

describe('DateFormatPipe', () => {
    it('create an instance', () => {
        const pipe = new DateFormatPipe('MMM DD, YYYY h:mm A');
        expect(pipe).toBeTruthy();
    });

    it('should add time zone indicator if one does not exist', () => {
        const pipe = new DateFormatPipe('MMM DD, YYYY h:mm A');

        expect(pipe.transform('2019-03-26T07:08:15.7757171')).toEqual('Mar 26, 2019 12:08 AM');
        expect(pipe.transform('2019-03-26T07:19:59.8337458Z')).toEqual('Mar 26, 2019 12:19 AM');
    });

    describe('override default settings', () => {
        let fixture: ComponentFixture<DateFormatTestComponent>;
        let component: DateFormatTestComponent;

        beforeEach(async(() => {
            TestBed.configureTestingModule({
                imports: [SlbDateFormatModule],
                declarations: [
                    DateFormatTestComponent
                ],
                providers: [
                    { provide: DEFAULT_DATE_FORMAT, useValue: 'YYYY-MM-DD' }
                ]
            })
                .compileComponents();
        }));

        beforeEach(() => {
            fixture = TestBed.createComponent(DateFormatTestComponent);
            component = fixture.componentInstance;
        });

        it('should format the date', () => {
            component.date = '2019-03-26T07:08:15.7757171';
            fixture.detectChanges();

            expect(fixture.debugElement.nativeElement.innerText).toEqual('2019-03-26');
        });
    });
});
