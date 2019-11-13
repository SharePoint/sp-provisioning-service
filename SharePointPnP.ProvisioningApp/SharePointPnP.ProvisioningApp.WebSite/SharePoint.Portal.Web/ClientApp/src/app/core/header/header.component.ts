import { Component, OnInit, OnDestroy, ElementRef, AfterViewInit, ViewChild } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Router, NavigationEnd } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';

import { appConstants } from 'src/app/app-constants';
import { CategoriesService } from '../api/services';

import { NavMenuItem } from './nav-menu-item/nav-menu-item';
import { NavMenuItemComponent } from './nav-menu-item/nav-menu-item.component';
import { DocumentClickService } from '../document-click.service';
import { AllMicrosoftMegamenuComponent } from './all-microsoft-megamenu/all-microsoft-megamenu.component';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApplicationSettingsService } from '../api/services/application-settings.service';

const designMenuText = 'View the designs';
const lookBookPdfItem: NavMenuItem = {
    name: 'Download look book PDF',
    externalLink: '/assets/SharePoint_lookbook_2019.pdf'
};

@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
})
export class HeaderComponent implements OnInit, AfterViewInit, OnDestroy {
    targetPlatformId: Observable<string>;

    navItems: NavMenuItem[];
    moreItem: NavMenuItem;

    isShowingMainMenu = false;
    isSmallScreen = false;
    isLargeScreen = false;

    @ViewChild('menuElement', { static: false }) private menuElement: ElementRef;

    @ViewChild('megamenuElement', { static: true }) private megamenuElement: ElementRef;
    @ViewChild('megamenuTogglerElement', { static: true }) private megamenuTogglerElement: ElementRef;
    @ViewChild('microsoftMegamenu', { static: false }) private microsoftMegamenu: AllMicrosoftMegamenuComponent;
    private currentOpenMenu: NavMenuItemComponent;
    private categoriesMenuItem: NavMenuItem;

    private destroy = new Subject<void>();

    constructor(
        private categoriesService: CategoriesService,
        private breakPointObserver: BreakpointObserver,
        private documentClickService: DocumentClickService,
        private appSettings: ApplicationSettingsService,
        private router: Router,
    ) { }

    ngOnInit() {
        this.categoriesMenuItem = {
            name: designMenuText
        };
        this.navItems = [this.categoriesMenuItem, lookBookPdfItem];
        this.overflowUpdate();

        this.categoriesService.getAll()
            .subscribe(categories => {
                this.categoriesMenuItem.subItems = categories.map((category): NavMenuItem => ({
                    name: category.displayName,
                    subItems: category.packages.map((packageInfo): NavMenuItem => ({
                        name: packageInfo.displayName,
                        routerLink: ['/details', packageInfo.id]
                    }))
                }));
            });

        this.router.events
            .pipe(
                takeUntil(this.destroy),
                filter(event => event instanceof NavigationEnd)
            )
            .subscribe(() => this.closeMainMenu());

        this.isSmallScreen = !this.breakPointObserver.isMatched(appConstants.mediaBreakpointUp.md);
        this.isLargeScreen = this.breakPointObserver.isMatched(appConstants.mediaBreakpointUp.lg);

        this.targetPlatformId = this.appSettings
            .getSettings()
            .pipe(map(settings => settings.targetPlatformId));
    }

    ngAfterViewInit() {
        this.breakPointObserver
            .observe(
                [appConstants.mediaBreakpointUp.md, appConstants.mediaBreakpointUp.lg, appConstants.mediaBreakpointUp.xl]
            )
            .pipe(takeUntil(this.destroy))
            .subscribe(state => {
                if (this.isSmallScreen === state.matches) {
                    this.isSmallScreen = !state.matches;
                    this.closeMainMenu();
                    this.closeMicrosoftMegamenu();
                }
                this.isLargeScreen = state.breakpoints[appConstants.mediaBreakpointUp.lg];
                this.overflowUpdate();
            });

        this.documentClickService.click
            .pipe(
                takeUntil(this.destroy),
                filter(() => this.hasMenuOpen())
            )
            .subscribe(event => this.handleDocumentClick(event));
    }

    ngOnDestroy() {
        this.destroy.next();
        this.destroy.complete();
    }

    toggleMainMenu() {
        if (!this.isShowingMainMenu) {
            this.openMainMenu();
        } else {
            this.closeMainMenu();
        }
    }

    openMainMenu() {
        if (!this.isSmallScreen || this.isShowingMainMenu) {
            // Do not open main menu if screen is not small
            return;
        }
        this.isShowingMainMenu = true;
    }

    closeMainMenu() {
        this.isShowingMainMenu = false;
        if (this.currentOpenMenu) {
            this.currentOpenMenu.close();
            this.currentOpenMenu = null;
        }
    }

    closeMicrosoftMegamenu() {
        if (this.microsoftMegamenu) {
            this.microsoftMegamenu.mainPanel.close();
        }
    }

    handleMenuOpened(menu: NavMenuItemComponent) {
        if (this.currentOpenMenu && this.currentOpenMenu !== menu) {
            this.currentOpenMenu.close();
        }
        this.currentOpenMenu = menu;
    }

    /**
     * Returns true if any menus are open
     */
    private hasMenuOpen() {
        return this.isShowingMainMenu ||
            (this.currentOpenMenu && this.currentOpenMenu.isOpen) ||
            this.microsoftMegamenu.mainPanel.isOpen;
    }

    /**
     * Closes any menus if clicked outside
     * @param event
     */
    private handleDocumentClick({ target: eventTarget }: MouseEvent) {
        const target = eventTarget as HTMLElement;
        if (!this.isSelfOrContained(this.menuElement, target)) {
            this.closeMainMenu();
        }
        if (this.microsoftMegamenu &&
            !this.isSelfOrContained(this.megamenuTogglerElement, target) &&
            !this.isSelfOrContained(this.megamenuElement, target)) {
            this.microsoftMegamenu.mainPanel.close();
        }
    }

    /**
     * Returns true if the element to check is the same as the self element,
     * or is contained in the self element
     * @param self
     * @param elemToCheck
     */
    private isSelfOrContained(self: ElementRef, elemToCheck: HTMLElement) {
        const selfElem = self.nativeElement as HTMLElement;
        return selfElem === elemToCheck ||
            selfElem.contains(elemToCheck);
    }

    /**
     * Moves items between the main top nav and the overflow menu
     * @param itemLimit
     */
    private overflowUpdate() {
        if (!this.navItems) {
            return;
        }

        if (!this.breakPointObserver.isMatched(appConstants.mediaBreakpointUp.md)) {
            // Navigation is in collapsed state so add all items back
            if (this.moreItem) {
                this.navItems.push(...this.moreItem.subItems);
                this.moreItem = null;
            }
            return;
        }

        let itemLimit = 0;
        if (this.breakPointObserver.isMatched(appConstants.mediaBreakpointUp.xl)) {
            itemLimit = 5;
        } else if (this.breakPointObserver.isMatched(appConstants.mediaBreakpointUp.lg)) {
            itemLimit = 2;
        }

        const capacityDifference = itemLimit - this.navItems.length;

        if (capacityDifference < -1) {
            const removedItems = this.navItems.splice(itemLimit);
            if (!this.moreItem) {
                this.moreItem = {
                    name: 'More',
                    subItems: removedItems
                };
            } else {
                this.moreItem.subItems.splice(0, 0, ...removedItems);
            }

        } else if (capacityDifference > 0 && this.moreItem) {
            if (this.moreItem.subItems.length > capacityDifference + 1) {
                const toAddItems = this.moreItem.subItems.splice(0, capacityDifference);
                this.navItems.push(...toAddItems);
            } else {
                this.navItems.push(...this.moreItem.subItems);
                this.moreItem = null;
            }
        }
    }
}
