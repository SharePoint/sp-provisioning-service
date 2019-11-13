import { Pipe, PipeTransform, Inject } from '@angular/core';
import * as moment from 'moment';

import { DEFAULT_DATE_FORMAT } from './default-date-format.token';

@Pipe({
    name: 'slbDateFormat'
})
export class DateFormatPipe implements PipeTransform {
    defaultFormat: string;

    constructor(
        @Inject(DEFAULT_DATE_FORMAT) dateFormat: string
    ) {
        this.defaultFormat = dateFormat;
    }

    transform(value: string | moment.Moment | Date, format: string = null): string {
        let toFormat: moment.Moment;
        format = format || this.defaultFormat;

        if (typeof value === 'string' && !value.endsWith('Z') && value.search(/(\+|\-)[0-9]{2}:[0-9]{2}/i) === -1) {
            // If timezone not found, assume UTC
            toFormat = moment(value + 'Z');
        } else {
            toFormat = moment(value);
        }

        return toFormat.format(format);
    }
}
