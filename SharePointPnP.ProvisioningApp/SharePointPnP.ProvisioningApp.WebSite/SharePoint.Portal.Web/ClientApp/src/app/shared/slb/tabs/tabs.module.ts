import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PortalModule } from '@angular/cdk/portal';
import { A11yModule } from '@angular/cdk/a11y';

import { TabGroupComponent } from './tab-group/tab-group.component';
import { TabComponent } from './tab/tab.component';
import { TabLabelComponent } from './tab-label/tab-label.component';
import { TabBodyComponent } from './tab-body/tab-body.component';

@NgModule({
    exports: [
        TabGroupComponent,
        TabComponent,
    ],
    declarations: [
        TabGroupComponent,
        TabComponent,
        TabLabelComponent,
        TabBodyComponent
    ],
    imports: [
        CommonModule,
        A11yModule,
        PortalModule,
    ]
})
export class SlbTabsModule { }
