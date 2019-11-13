import {
    Input, Component, EventEmitter, Output, HostBinding,
    OnDestroy, AfterContentInit, AfterContentChecked,
    ContentChildren, QueryList, ChangeDetectionStrategy, ViewChildren, AfterViewInit,
} from '@angular/core';
import { FocusKeyManager } from '@angular/cdk/a11y';
import { hasModifierKey } from '@angular/cdk/keycodes';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { TabComponent } from '../tab/tab.component';
import { TabLabelComponent } from '../tab-label/tab-label.component';

const componentSelector = 'slb-tab-group';

@Component({
    selector: componentSelector,
    templateUrl: './tab-group.component.html',
})
export class TabGroupComponent implements OnDestroy, AfterViewInit, AfterContentInit, AfterContentChecked {
    @Output() readonly selectedIndexChange = new EventEmitter<number>();
    @Input() alignLabels: 'center' | 'left' | 'right';

    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;

    @ContentChildren(TabComponent) tabs: QueryList<TabComponent>;

    @ViewChildren(TabLabelComponent) private tabLabels: QueryList<TabLabelComponent>;
    private keyManager: FocusKeyManager<TabLabelComponent>;

    private indexToSelect: number;
    private selectedIndex: number;
    private componentDestroy = new Subject<void>();

    get alignClass(): string {
        if (this.alignLabels) {
            return `align-${this.alignLabels}`;
        }
    }

    constructor() { }

    ngOnDestroy() {
        this.componentDestroy.next();
        this.componentDestroy.complete();
    }

    ngAfterContentInit() {
        this.tabs.changes
            .pipe(takeUntil(this.componentDestroy))
            .subscribe(() => {
                let indexToSelect = 0;
                this.tabs.forEach((item, index) => {
                    if (item.isActive) {
                        indexToSelect = index;
                    }
                });

                this.updateTabsActiveState(indexToSelect);

                if (this.selectedIndex !== indexToSelect) {
                    this.selectedIndex = this.indexToSelect = indexToSelect;
                    this.selectedIndexChange.emit(this.selectedIndex);
                }
            });
    }

    ngAfterContentChecked() {
        const maxIndex = this.tabs.length - 1;
        const indexToSelect = this.clampIndex(this.indexToSelect, maxIndex);

        if (this.selectedIndex !== indexToSelect) {
            this.updateTabsActiveState(indexToSelect);
            this.selectedIndex = indexToSelect;
            this.selectedIndexChange.emit(this.selectedIndex);
        }
    }

    ngAfterViewInit() {
        this.keyManager = new FocusKeyManager<TabLabelComponent>(this.tabLabels)
            .withHorizontalOrientation('ltr')
            .withWrap();

        this.keyManager.updateActiveItem(0);

        this.keyManager.change
            .pipe(takeUntil(this.componentDestroy))
            .subscribe(index => this.tabLabels.toArray()[index].focus());
    }

    activateTab(index: number) {
        this.indexToSelect = index;
    }

    handleLabelWrapperKeydown(event: KeyboardEvent) {
        if (hasModifierKey(event)) {
            return;
        }

        switch (event.key) {
            case 'Home':
                this.keyManager.setFirstItemActive();
                event.preventDefault();
                break;
            case 'End':
                this.keyManager.setLastItemActive();
                event.preventDefault();
                break;
            case 'Enter':
                this.activateTab(this.keyManager.activeItemIndex);
                event.preventDefault();
                break;
            default:
                this.keyManager.onKeydown(event);
        }
    }

    private clampIndex(index: number, maxIndex: number) {
        index = index || 0;
        if (index > maxIndex) {
            index = maxIndex;
        } else if (index < 0) {
            index = 0;
        }
        return index;
    }

    private updateTabsActiveState(indexToSelect: number) {
        this.tabs.forEach((tab, index) => {
            tab.isActive = index === indexToSelect;
        });

        if (this.keyManager) {
            this.keyManager.updateActiveItem(indexToSelect);
        }
    }
}
