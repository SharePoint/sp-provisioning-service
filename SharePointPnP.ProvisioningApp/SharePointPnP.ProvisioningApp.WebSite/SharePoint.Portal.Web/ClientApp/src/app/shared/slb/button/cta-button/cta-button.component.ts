import { Component, HostBinding, Input } from '@angular/core';

type ButtonType = 'link' | 'primary';

@Component({
    // tslint:disable-next-line: component-selector
    selector: 'a[slb-cta-button], button[slb-cta-button]',
    templateUrl: './cta-button.component.html',
})
export class CtaButtonComponent {
    @Input() buttonType: ButtonType = 'primary';

    @HostBinding('class.call-to-action') readonly cssCtaClass = true;
    @HostBinding('class.slb-primary-button') get isPrimary(): boolean {
        return this.buttonType === 'primary';
    }
    @HostBinding('class.slb-link-button') get isLink(): boolean {
        return this.buttonType === 'link';
    }

    constructor() { }
}
