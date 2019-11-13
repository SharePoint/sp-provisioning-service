import { Directive, TemplateRef, ViewContainerRef, OnInit, Input, OnDestroy, OnChanges } from '@angular/core';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { DetailsPageTemplateDataService } from './details-page-template-data.service';
import { DetailItemCategory } from 'src/app/core/api/models';

@Directive({
    // tslint:disable-next-line: directive-selector
    selector: '[slbDisplayForCategory]'
})
export class DisplayForCategoryDirective implements OnInit, OnChanges, OnDestroy {
    @Input('slbDisplayForCategory') categoryName: string;

    private categories: DetailItemCategory[];
    private readonly destroy = new Subject<void>();

    constructor(
        private dataService: DetailsPageTemplateDataService,
        private templateRef: TemplateRef<any>,
        private viewContainerRef: ViewContainerRef,
    ) { }

    ngOnInit() {
        this.dataService.data
            .pipe(takeUntil(this.destroy))
            .subscribe(data => {
                this.categories = data.detailItemCategories;
                this.updateView();
            });
    }

    ngOnChanges() {
        this.updateView();
    }

    ngOnDestroy() {
        this.destroy.next();
        this.destroy.complete();
    }

    private updateView() {
        this.viewContainerRef.clear();
        if (!this.categories || !this.categories.length || !this.categoryName) {
            return;
        }

        const selectedCategory = this.categories.find(category => category.name.toLowerCase() === this.categoryName.toLowerCase());

        if (selectedCategory) {
            const name = selectedCategory.name;
            const items = selectedCategory.items;

            if (items && items.length) {
                this.viewContainerRef.createEmbeddedView(this.templateRef, {
                    $implicit: selectedCategory,
                    items,
                    name
                });
            }
        }
    }
}
