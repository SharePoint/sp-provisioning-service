import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DynamicTemplateComponent } from './dynamic-template.component';

@NgModule({
    exports: [
        DynamicTemplateComponent
    ],
    declarations: [
        DynamicTemplateComponent
    ],
    imports: [
        CommonModule
    ]
})
export class DynamicTemplateModule { }
