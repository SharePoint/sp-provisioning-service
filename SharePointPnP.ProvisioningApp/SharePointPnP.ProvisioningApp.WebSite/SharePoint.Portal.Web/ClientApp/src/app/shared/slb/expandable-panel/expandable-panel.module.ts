import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ExpandablePanelHeaderComponent } from './expandable-panel-header/expandable-panel-header.component';
import { ExpandablePanelComponent } from './expandable-panel/expandable-panel.component';

@NgModule({
    exports: [
        ExpandablePanelHeaderComponent,
        ExpandablePanelComponent
    ],
    declarations: [
        ExpandablePanelHeaderComponent,
        ExpandablePanelComponent
    ],
    imports: [
        CommonModule
    ]
})
export class SlbExpandablePanelModule { }
