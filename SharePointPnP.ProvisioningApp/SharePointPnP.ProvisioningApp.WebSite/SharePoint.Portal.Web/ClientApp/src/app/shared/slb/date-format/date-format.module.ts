import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DateFormatPipe } from './date-format.pipe';
import { DEFAULT_DATE_FORMAT } from './default-date-format.token';

@NgModule({
    exports: [
        DateFormatPipe
    ],
    declarations: [
        DateFormatPipe
    ],
    imports: [
        CommonModule
    ],
    providers: [
        { provide: DEFAULT_DATE_FORMAT, useValue: 'MMM D, YYYY h:mm A' }
    ]
})
export class SlbDateFormatModule { }
