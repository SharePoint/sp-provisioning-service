import { Injectable } from '@angular/core';
import { Observable, fromEvent } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class DocumentClickService {
    get click(): Observable<MouseEvent> {
        if (!this.clickStream) {
            this.clickStream = fromEvent(document, 'click');
        }
        return this.clickStream
            .pipe(map(event => event as MouseEvent));
    }

    private clickStream: Observable<Event>;
}
