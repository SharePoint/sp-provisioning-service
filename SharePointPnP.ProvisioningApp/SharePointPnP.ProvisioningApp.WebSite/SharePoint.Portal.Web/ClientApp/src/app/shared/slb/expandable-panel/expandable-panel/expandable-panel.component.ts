import { Component, HostBinding, Input, Output, EventEmitter } from '@angular/core';
import { AnimationTriggerMetadata, style, trigger, state, transition, animate } from '@angular/animations';

export const panelAnimationDuration = 250;

const expandPanel: AnimationTriggerMetadata = trigger('expandPanel', [
    state('expanded', style({ height: '*' })),
    state('collapsed, void', style({ height: '0px' })),
    transition('void => *, * => void', animate(panelAnimationDuration)),
]);

const componentSelector = 'slb-expandable-panel';
@Component({
    selector: componentSelector,
    templateUrl: './expandable-panel.component.html',
    styleUrls: ['./expandable-panel.component.scss'],
    animations: [expandPanel]
})
export class ExpandablePanelComponent {
    @Input() set expanded(val: boolean) {
        this.isExpanded = val;
        this.expandedChange.next(this.isExpanded);
    }
    get expanded(): boolean {
        return this.isExpanded;
    }

    @Output() expandedChange = new EventEmitter<boolean>();

    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;

    private isExpanded = false;

    toggle() {
        this.expanded = !this.isExpanded;
    }
}
