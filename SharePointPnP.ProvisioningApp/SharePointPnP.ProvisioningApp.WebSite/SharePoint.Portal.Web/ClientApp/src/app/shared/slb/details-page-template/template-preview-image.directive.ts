import { Directive, HostBinding, OnInit, OnDestroy, Input, OnChanges, SimpleChange, SimpleChanges } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { PreviewImage } from 'src/app/core/api/models';
import { DetailsPageTemplateDataService } from './details-page-template-data.service';

const directiveSelector = 'slbTemplatePreviewImage';

@Directive({
    // tslint:disable-next-line: directive-selector
    selector: `img[${directiveSelector}]`
})
export class TemplatePreviewImageDirective implements OnInit, OnChanges, OnDestroy {
    @Input(directiveSelector) previewImageType = 'fullpage';

    @HostBinding('attr.src') imgSrc: string;
    @HostBinding('attr.alt') altText: string;

    @HostBinding('class.slb-template-preview-image') readonly cssClass = true;

    private previewImages: PreviewImage[];
    private destroy = new Subject<void>();

    constructor(
        private dataService: DetailsPageTemplateDataService
    ) { }

    ngOnInit() {
        this.previewImageType = this.previewImageType || 'fullpage';
        this.dataService.data
            .pipe(takeUntil(this.destroy))
            .subscribe(data => {
                this.previewImages = data.previewImages;
                this.updateImage();
            });
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes.previewImageType.firstChange) {
            return;
        }
        this.previewImageType = this.previewImageType || 'fullpage';
        this.updateImage();
    }

    ngOnDestroy() {
        this.destroy.next();
        this.destroy.complete();
    }

    private updateImage() {
        const image = this.previewImages.find(previewImage => previewImage.type === this.previewImageType);
        if (image) {
            this.imgSrc = image.url;
            this.altText = image.altText;
        } else {
            this.imgSrc = this.altText = undefined;
        }
    }
}
