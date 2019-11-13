import { Component, HostBinding, Host, HostListener } from '@angular/core';
import { AnimationTriggerMetadata, trigger, state, style, animate, transition } from '@angular/animations';

import { ExpandablePanelComponent, panelAnimationDuration } from '../expandable-panel/expandable-panel.component';

const rotateIndicator: AnimationTriggerMetadata = trigger('rotateIndicator', [
    state('expanded', style({ transform: 'rotate(180deg)' })),
    state('collapsed', style({ transform: 'rotate(0deg)' })),
    transition('* => *', animate(panelAnimationDuration)),
]);

const componentSelector = 'slb-expandable-panel-header';
@Component({
    selector: componentSelector,
    templateUrl: './expandable-panel-header.component.html',
    styleUrls: ['./expandable-panel-header.component.scss'],
    animations: [rotateIndicator]
})
export class ExpandablePanelHeaderComponent {
    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;

    get isExpanded() {
        return this.panel.expanded;
    }

    get expansionState() {
        return this.panel.expanded ? 'expanded' : 'collapsed';
    }

    constructor(
        @Host() private panel: ExpandablePanelComponent
    ) { }

    @HostListener('click') toggle() {
        this.panel.toggle();
    }
}
