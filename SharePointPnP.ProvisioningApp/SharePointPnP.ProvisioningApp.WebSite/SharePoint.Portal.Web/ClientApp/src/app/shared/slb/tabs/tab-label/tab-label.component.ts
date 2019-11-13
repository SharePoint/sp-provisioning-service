import { Component, HostBinding, ChangeDetectionStrategy, ElementRef, Input } from '@angular/core';
import { FocusableOption } from '@angular/cdk/a11y';

const componentSelector = 'slb-tab-label';

@Component({
    selector: componentSelector,
    templateUrl: './tab-label.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TabLabelComponent implements FocusableOption {
    @Input() label: string;

    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;

    constructor(
        private elementRef: ElementRef
    ) { }

    focus(): void {
        this.elementRef.nativeElement.focus();
    }
}
