import { Component, ViewChildren, QueryList, Input, ViewChild } from '@angular/core';

import { NavMenuPanelDirective } from '../nav-menu-panel.directive';

@Component({
    selector: 'app-all-microsoft-megamenu',
    templateUrl: './all-microsoft-megamenu.component.html',
    exportAs: 'allMicrosoftMegamenu'
})
export class AllMicrosoftMegamenuComponent {
    @Input() set megamenuPanelButtonsTabbable(areTabbable: boolean) {
        this.tabIndex = areTabbable ? null : -1;
    }

    tabIndex = null;

    @ViewChild('mainPanel', { static: true }) mainPanel: NavMenuPanelDirective;
    @ViewChildren(NavMenuPanelDirective) private panels: QueryList<NavMenuPanelDirective>;

    /**
     * Closes all child panels when main panel is closed
     */
    handleMainPanelClosed() {
        if (this.panels) {
            this.panels.forEach(panel => panel.close());
        }
    }
}
