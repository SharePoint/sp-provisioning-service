import { Component } from '@angular/core';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Subject } from 'rxjs';
import { DetailsPageTemplateData, DetailsPageTemplateDataService } from './details-page-template-data.service';
import { SlbDetailsPageTemplateModule } from './details-page-template.module';

@Component({
    template: `<img [slbTemplatePreviewImage]="type">`
})
class TemplatePreviewImageTestComponent {
    type: string;
}

describe('TemplatePreviewImageDirective', () => {
    let component: TemplatePreviewImageTestComponent;
    let fixture: ComponentFixture<TemplatePreviewImageTestComponent>;

    let testData: DetailsPageTemplateData;
    let dataSubject: Subject<DetailsPageTemplateData>;

    beforeEach(async(() => {
        testData = new DetailsPageTemplateData();

        dataSubject = new Subject<DetailsPageTemplateData>();
        const dataServiceMock = {
            data: dataSubject
        };

        TestBed.configureTestingModule({
            declarations: [TemplatePreviewImageTestComponent],
            imports: [SlbDetailsPageTemplateModule],
            providers: [
                { provide: DetailsPageTemplateDataService, useValue: dataServiceMock },
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(TemplatePreviewImageTestComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create an instance', () => expect(component).toBeTruthy());

    it('should set the image source and alt text attributes', () => {
        const src = 'http://imagesrc';
        const altText = 'Image alt text';
        testData.previewImages = [
            {
                type: 'fullpage',
                altText,
                url: src
            }
        ];
        dataSubject.next(testData);
        fixture.detectChanges();

        const elem = fixture.debugElement.query(By.css('img')).nativeElement as HTMLElement;

        expect(elem.getAttribute('src')).toEqual(src);
        expect(elem.getAttribute('alt')).toEqual(altText);
    });

    it('should be able to select other types of images', () => {
        const src = 'http://imagesrc';
        const altText = 'Image alt text';
        testData.previewImages = [
            {
                type: 'fullpage',
                altText: 'Not the alt text we want',
                url: 'some other url'
            },
            {
                type: 'cardpreview',
                altText,
                url: src
            },
        ];

        component.type = 'cardpreview';
        dataSubject.next(testData);
        fixture.detectChanges();

        const elem = fixture.debugElement.query(By.css('img')).nativeElement as HTMLElement;

        expect(elem.getAttribute('src')).toEqual(src);
        expect(elem.getAttribute('alt')).toEqual(altText);
    });
});
