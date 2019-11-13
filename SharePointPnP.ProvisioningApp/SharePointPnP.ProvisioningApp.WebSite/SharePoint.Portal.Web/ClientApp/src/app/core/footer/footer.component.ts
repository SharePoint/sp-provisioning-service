import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ApplicationSettingsService } from '../api/services/application-settings.service';

@Component({
    selector: 'app-footer',
    templateUrl: './footer.component.html'
})
export class FooterComponent implements OnInit {
    currentDate: Observable<string>;
    targetPlatformId: Observable<string>;

    constructor(
        private appSettings: ApplicationSettingsService
    ) { }

    ngOnInit() {
        this.currentDate = this.appSettings
            .getSettings()
            .pipe(map(settings => settings.serverDateTime));

        this.targetPlatformId = this.appSettings
            .getSettings()
            .pipe(map(settings => settings.targetPlatformId));
    }
}
