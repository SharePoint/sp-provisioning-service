import { Component, OnInit, Input, OnChanges, SimpleChanges, HostBinding, ChangeDetectionStrategy } from '@angular/core';
import { trigger, AnimationTriggerMetadata, style, state, transition, animate, keyframes } from '@angular/animations';
import { TemplatePortal } from '@angular/cdk/portal';

type ItemAnimationState = 'left' | 'center' | 'right' | 'leftToCenter' | 'rightToCenter';

/**
 * Translate animation adopted from angular material tabs.
 * All animations have keyframes or else they don't work properly on edge if there is a
 * mix between keyframes and letting it calculate.
 */
const translateItem: AnimationTriggerMetadata = trigger('translateItem', [
    state('center, leftToCenter, rightToCenter, void', style({ transform: 'none' })),

    state('left', style({ transform: 'translate3d(-100%, 0, 0)', minHeight: '1px' })),
    state('right', style({ transform: 'translate3d(100%, 0, 0)', minHeight: '1px' })),

    transition('* => center',
        animate('{{animationDuration}} cubic-bezier(0.35, 0, 0.25, 1)',
            style({ transform: 'none' })
        )),

    transition('center => right, leftToCenter => right, rightToCenter => right',
        animate('{{animationDuration}} cubic-bezier(0.35, 0, 0.25, 1)',
            keyframes([
                style({ transform: 'none' }),
                style({ transform: 'translate3d(100%, 0, 0)' }),
            ])
        )),
    transition('center => left, leftToCenter => left, rightToCenter => left',
        animate('{{animationDuration}} cubic-bezier(0.35, 0, 0.25, 1)',
            keyframes([
                style({ transform: 'none' }),
                style({ transform: 'translate3d(-100%, 0, 0)' }),
            ])
        )),
    transition('* => leftToCenter',
        animate('{{animationDuration}} cubic-bezier(0.35, 0, 0.25, 1)',
            keyframes([
                style({ transform: 'translate3d(-100%, 0, 0)' }),
                style({ transform: 'none' }),
            ])
        )),
    transition('* => rightToCenter',
        animate('{{animationDuration}} cubic-bezier(0.35, 0, 0.25, 1)',
            keyframes([
                style({ transform: 'translate3d(100%, 0, 0)' }),
                style({ transform: 'none' }),
            ])
        )),
]);

const componentSelector = 'slb-carousel-item-body';

@Component({
    selector: componentSelector,
    templateUrl: './carousel-item-body.component.html',
    styleUrls: ['./carousel-item-body.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: [translateItem]
})
export class CarouselItemBodyComponent implements OnInit, OnChanges {
    @Input() content: TemplatePortal;
    @Input() isActive: boolean;
    @Input() animationDuration = '500ms';
    @Input() direction: 'left' | 'right';

    @HostBinding(`class.${componentSelector}`) readonly cssClass = true;

    animationState: ItemAnimationState;

    constructor() { }

    ngOnInit() {
        this.calculateAnimationState();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes.isActive && !changes.isActive.isFirstChange()) {
            this.calculateAnimationState();
        }
    }

    private calculateAnimationState() {
        if (this.isActive) {
            switch (this.direction) {
                case 'left':
                    this.animationState = 'leftToCenter';
                    return;
                case 'right':
                    this.animationState = 'rightToCenter';
                    return;
                default:
                    this.animationState = 'center';
            }
        } else if (this.direction === 'left') {
            this.animationState = 'right';
        } else {
            this.animationState = 'left';
        }
    }
}
