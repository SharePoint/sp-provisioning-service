import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OverlayModule } from '@angular/cdk/overlay';
import { PortalModule } from '@angular/cdk/portal';

import { StickyHeaderComponent } from './sticky-header.component';
import { StickyHeaderContainerComponent } from './sticky-header-container/sticky-header-container.component';
import { StickyHeaderContentClassDirective } from './sticky-header-content-class.directive';

@NgModule({
    exports: [
        StickyHeaderComponent,
        StickyHeaderContentClassDirective,
    ],
    declarations: [
        StickyHeaderComponent,
        StickyHeaderContainerComponent,
        StickyHeaderContentClassDirective,
    ],
    imports: [
        CommonModule,
        OverlayModule,
        PortalModule,
    ],
    entryComponents: [
        StickyHeaderContainerComponent,
    ]
})
export class SlbStickyHeaderModule { }
