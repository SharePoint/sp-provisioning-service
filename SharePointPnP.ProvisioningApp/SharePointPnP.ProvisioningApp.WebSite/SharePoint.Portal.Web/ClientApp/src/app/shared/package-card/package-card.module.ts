import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { PackageCardComponent } from './package-card.component';

@NgModule({
    exports: [
        PackageCardComponent
    ],
    declarations: [
        PackageCardComponent
    ],
    imports: [
        CommonModule,
        RouterModule,
    ]
})
export class PackageCardModule { }
