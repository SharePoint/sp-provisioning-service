import {
    Component, ContentChildren, QueryList, Input, AfterContentInit,
    AfterContentChecked, HostBinding, ChangeDetectionStrategy,
    Output, EventEmitter, OnDestroy, ChangeDetectorRef,
} from '@angular/core';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { CarouselItemComponent } from './carousel-item/carousel-item.component';

const componentSelector = 'slb-carousel';

@Component({
    selector: componentSelector,
    templateUrl: './carousel.component.html',
    styleUrls: ['./carousel.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CarouselComponent implements OnDestroy, AfterContentInit, AfterContentChecked {
    @Input() animationDuration = '500ms';
    @Input() doLoopItems = true;
    @Output() readonly selectedIndexChange = new EventEmitter<number>();

    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;

    @ContentChildren(CarouselItemComponent) items: QueryList<CarouselItemComponent>;

    doShowLeftButton: boolean;
    doShowRightButton: boolean;
    direction: 'left' | 'right';

    private indexToSelect: number;
    private selectedIndex: number;
    private componentDestroy = new Subject<void>();

    constructor(
        private changeDetectorRef: ChangeDetectorRef
    ) { }

    ngOnDestroy() {
        this.componentDestroy.next();
        this.componentDestroy.complete();
    }

    ngAfterContentInit() {
        this.items.changes
            .pipe(takeUntil(this.componentDestroy))
            .subscribe(() => {
                let indexToSelect = 0;
                this.items.forEach((item, index) => {
                    if (item.isActive) {
                        indexToSelect = index;
                    }
                });

                this.updateItemsActiveState(indexToSelect);

                if (this.selectedIndex !== indexToSelect) {
                    this.selectedIndex = this.indexToSelect = indexToSelect;
                    this.selectedIndexChange.emit(this.selectedIndex);
                }

                this.direction = null;
                this.changeDetectorRef.markForCheck();
            });
    }

    ngAfterContentChecked() {
        const maxIndex = this.calcMaxIndex();
        const indexToSelect = this.loopIndex(this.indexToSelect, maxIndex);

        if (!this.doLoopItems) {
            this.doShowLeftButton = indexToSelect !== 0;
            this.doShowRightButton = indexToSelect !== maxIndex;
        } else {
            this.doShowLeftButton = this.doShowRightButton = this.items.length > 1;
        }

        if (this.selectedIndex !== indexToSelect) {
            this.updateItemsActiveState(indexToSelect);
            this.selectedIndex = indexToSelect;
            this.selectedIndexChange.emit(this.selectedIndex);
        }
    }

    goLeft() {
        this.indexToSelect = this.selectedIndex - 1;
        this.direction = 'left';
    }

    goRight() {
        this.indexToSelect = this.selectedIndex + 1;
        this.direction = 'right';
    }

    /**
     * Calculate the maximum index
     */
    private calcMaxIndex(): number {
        return this.items.length - 1;
    }

    /**
     * Sets the index back to 0 if we go beyond the end, or the index to max
     * if we go beyond the beginning
     * @param index
     */
    private loopIndex(index: number, maxIndex: number) {
        index = index || 0;
        if (index > maxIndex) {
            index = 0;
        } else if (index < 0) {
            index = maxIndex;
        }
        return index;
    }

    private updateItemsActiveState(indexToSelect: number) {
        this.items.forEach((item, index) => {
            item.isActive = index === indexToSelect;
        });
    }
}
