import {
    Component, Input, Output, EventEmitter, ViewChild,
    ElementRef, Renderer2, AfterViewChecked
} from '@angular/core';
import { Router } from '@angular/router';

import { NavMenuItem } from './nav-menu-item';
import { NavMenuPanelDirective } from '../nav-menu-panel.directive';

@Component({
    selector: 'app-nav-menu-item',
    templateUrl: './nav-menu-item.component.html',
})
export class NavMenuItemComponent implements AfterViewChecked {
    @Input() item: NavMenuItem;
    @Input() opensOnHover: boolean;
    @Input() subMenuOpensOnHover: boolean;
    @Input() expandMenuHeight: boolean;

    @Output() opened = new EventEmitter<NavMenuItemComponent>();
    @Output() closed = new EventEmitter<NavMenuItemComponent>();
    @Output() itemSelected = new EventEmitter<NavMenuItem>();

    @ViewChild('navMenuPanel', { static: false }) menuPanel: NavMenuPanelDirective;
    @ViewChild('menuPanelElement', { static: false }) private menuPanelElement: ElementRef;

    get menuPanelHeight(): number {
        if (!this.menuPanelElement) {
            return 0;
        }

        const elem = this.menuPanelElement.nativeElement as HTMLElement;
        const boundingBox = elem.getBoundingClientRect();
        return boundingBox.height;
    }

    get isOpen(): boolean {
        return this.menuPanel && this.menuPanel.isOpen;
    }

    get target(): string {
        return this.item.opensInNewTab ? '_blank' : null;
    }

    private currentOpenMenu: NavMenuItemComponent;

    constructor(
        private renderer: Renderer2,
        private router: Router,
    ) { }

    ngAfterViewChecked() {
        this.updatePanelHeight();
    }

    open() {
        if (this.menuPanel) {
            this.menuPanel.open();
        }
    }

    close() {
        if (this.menuPanel) {
            this.menuPanel.close();
        }
    }

    handlePanelOpened() {
        this.opened.emit(this);
    }

    handlePanelClosed() {
        if (this.currentOpenMenu) {
            this.currentOpenMenu.close();
        }
        this.unsetMenuPanelHeight();
        this.closed.emit(this);
    }

    /**
     * Opens a submenu if it opens on hover
     */
    handleMouseEnterActivator() {
        if (this.opensOnHover) {
            this.open();
        }
    }

    /**
     * Closes an open sub menu if it opens on hover
     */
    handleMouseEnterLink() {
        if (this.opensOnHover) {
            this.opened.emit(this);
        }
    }

    /**
     * Emit when an item has been selected and navigate.
     * @param event
     * @param item
     */
    handleItemSelected(event: MouseEvent, item: NavMenuItem) {
        if (!item.routerLink) {
            this.itemSelected.emit(item);
        }
    }

    /**
     * Emit when a sub-item has been selected
     * @param item
     */
    handleSubItemSelected(item: NavMenuItem) {
        this.itemSelected.emit(item);
    }

    /**
     * Tracks the open submenu menu and closes any currently open sub menus
     * @param menu
     */
    handleSubMenuOpened(menu: NavMenuItemComponent) {
        if (this.currentOpenMenu && this.currentOpenMenu !== menu) {
            this.currentOpenMenu.close();
        }
        this.currentOpenMenu = menu;
    }

    /**
     * Sets the height of the menu panel
     * @param height
     */
    setMenuPanelHeight(height: number) {
        if (this.menuPanelElement) {
            this.renderer.setStyle(this.menuPanelElement.nativeElement, 'height', `${height}px`);
        }
    }

    /**
     * Unset the height of the menu panel
     */
    unsetMenuPanelHeight() {
        if (this.menuPanelElement) {
            this.renderer.setStyle(this.menuPanelElement.nativeElement, 'height', 'auto');
        }
    }

    private updatePanelHeight() {
        this.unsetMenuPanelHeight();
        if (!this.currentOpenMenu || !this.currentOpenMenu.isOpen || !this.expandMenuHeight) {
            return;
        }

        const selfHeight = this.menuPanelHeight;
        const subMenuHeights: number[] = [];
        let openMenu = this.currentOpenMenu;
        while (openMenu) {
            subMenuHeights.push(openMenu.menuPanelHeight);
            openMenu = openMenu.currentOpenMenu;
        }
        const heightToSet = Math.max(selfHeight, ...subMenuHeights);

        if (selfHeight < heightToSet) {
            this.setMenuPanelHeight(heightToSet);
        }

        openMenu = this.currentOpenMenu;
        while (openMenu) {
            if (openMenu.menuPanelHeight < heightToSet) {
                openMenu.setMenuPanelHeight(heightToSet);
            }
            openMenu = openMenu.currentOpenMenu;
        }
    }
}
