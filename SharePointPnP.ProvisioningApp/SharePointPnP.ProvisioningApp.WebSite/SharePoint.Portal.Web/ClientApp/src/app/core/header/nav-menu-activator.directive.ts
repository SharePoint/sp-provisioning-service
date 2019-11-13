import { Directive, Input, HostListener, HostBinding } from '@angular/core';

import { NavMenuPanelDirective } from './nav-menu-panel.directive';

@Directive({
    selector: '[appNavMenuActivatorFor]'
})
export class NavMenuActivatorDirective {
    @Input('appNavMenuActivatorFor') panel: NavMenuPanelDirective;

    @HostBinding('class.has-glyph') readonly hasGlyphClass = true;

    @HostBinding('class.is-open')
    @HostBinding('attr.aria-expanded')
    get isOpen(): boolean {
        return this.panel && this.panel.isOpen;
    }

    constructor() { }

    @HostListener('click') toggle() {
        this.panel.toggle();
    }
}
