import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { DisplayInfo, Package, PreviewImage } from 'src/app/core/api/models';
import { PackageCardComponent } from './package-card.component';

describe('PackageCardComponent', () => {
    let component: PackageCardComponent;
    let fixture: ComponentFixture<PackageCardComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [
                RouterTestingModule,
            ],
            declarations: [PackageCardComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(PackageCardComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should set the hero image', () => {
        const testImage = new PreviewImage();
        testImage.type = 'cardpreview';
        const testPackageData = new Package();
        testPackageData.displayInfo = new DisplayInfo();
        testPackageData.displayInfo.previewImages = [new PreviewImage(), testImage];
        component.packageData = testPackageData;

        component.ngOnChanges();

        expect(component.heroImage).toBe(testImage);
    });
});
