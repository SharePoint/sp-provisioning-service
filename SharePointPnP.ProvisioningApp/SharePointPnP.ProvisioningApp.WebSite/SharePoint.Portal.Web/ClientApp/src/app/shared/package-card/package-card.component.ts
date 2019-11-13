import { Component, OnInit, Input, OnChanges, HostBinding } from '@angular/core';

import { Package, PreviewImage } from 'src/app/core/api/models';

const componentSelector = 'app-package-card';

@Component({
    selector: componentSelector,
    templateUrl: './package-card.component.html',
    styleUrls: ['./package-card.component.scss']
})
export class PackageCardComponent implements OnInit, OnChanges {
    @Input() packageData: Package;

    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;

    heroImage: PreviewImage;

    constructor(
    ) { }

    ngOnInit() {
        this.updateHeroImage();
    }

    ngOnChanges() {
        this.updateHeroImage();
    }

    private updateHeroImage() {
        if (!this.packageData) {
            return;
        }

        if (this.packageData.displayInfo && this.packageData.displayInfo.previewImages) {
            this.heroImage = this.packageData.displayInfo.previewImages.find(img => img.type === 'cardpreview');
        }
    }
}
