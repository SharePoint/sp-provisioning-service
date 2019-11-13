import { TestBed, async, ComponentFixture } from '@angular/core/testing';
import { Component } from '@angular/core';
import { By } from '@angular/platform-browser';
import { Subject } from 'rxjs';

import { DetailsPageTemplateData, DetailsPageTemplateDataService } from './details-page-template-data.service';
import { SlbDetailsPageTemplateModule } from './details-page-template.module';
import { DetailItemCategory } from 'src/app/core/api/models';

@Component({
    template: `
    <ng-template [slbDisplayForCategory]="categoryName" let-category let-name="name">
        <span id="categoryName">{{ category.name }}</span>
        <span id="name">{{ name }}</span>
    </ng-template>
    `
})
class DisplayForCategoryTestComponent {
    categoryName: string;
}

describe('DisplayForCategoryDirective', () => {
    let fixture: ComponentFixture<DisplayForCategoryTestComponent>;
    let component: DisplayForCategoryTestComponent;

    let testData: DetailsPageTemplateData;
    let dataSubject: Subject<DetailsPageTemplateData>;

    beforeEach(async(() => {
        testData = new DetailsPageTemplateData();

        dataSubject = new Subject<DetailsPageTemplateData>();
        const dataServiceMock = {
            data: dataSubject
        };

        TestBed.configureTestingModule({
            imports: [SlbDetailsPageTemplateModule],
            declarations: [DisplayForCategoryTestComponent],
            providers: [
                { provide: DetailsPageTemplateDataService, useValue: dataServiceMock },
            ]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(DisplayForCategoryTestComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create an instance', () => expect(component).toBeTruthy());

    it('should set the context with the correct category', () => {
        component.categoryName = 'category 1';
        const testCategory1 = new DetailItemCategory();
        testCategory1.name = 'Category 1';
        const testCategory2 = new DetailItemCategory();
        testCategory2.name = 'Category 2';
        testData.detailItemCategories = [testCategory2, testCategory1];
        dataSubject.next(testData);
        fixture.detectChanges();

        const categoryNameElem = fixture.debugElement.query(By.css('#categoryName'));
        const nameElem = fixture.debugElement.query(By.css('#name'));

        expect(categoryNameElem.nativeElement.innerText).toEqual('Category 1');
        expect(nameElem.nativeElement.innerText).toEqual('Category 1');
    });
});
