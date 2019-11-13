import { Directive, HostBinding, Output, EventEmitter } from '@angular/core';

@Directive({
    selector: '[appNavMenuPanel]',
    exportAs: 'navMenuPanel'
})
export class NavMenuPanelDirective {
    @Output() closed = new EventEmitter<void>();
    @Output() opened = new EventEmitter<void>();

    @HostBinding('class.menu-panel') readonly cssClass = true;
    @HostBinding('class.is-open') isOpen = false;

    constructor() { }

    toggle() {
        if (!this.isOpen) {
            this.open();
        } else {
            this.close();
        }
    }

    open() {
        if (!this.isOpen) {
            this.isOpen = true;
            this.opened.emit();
        }
    }

    close() {
        if (this.isOpen) {
            this.isOpen = false;
            this.closed.emit();
        }
    }
}
