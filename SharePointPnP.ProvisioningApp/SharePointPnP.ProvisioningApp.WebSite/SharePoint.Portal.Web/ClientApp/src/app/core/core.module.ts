import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { SlbButtonModule } from '../shared/slb/button/button.module';
import { SlbDateFormatModule } from '../shared/slb/date-format/date-format.module';

import { FooterComponent } from './footer/footer.component';
import { HeaderComponent } from './header/header.component';
import { AllMicrosoftMegamenuComponent } from './header/all-microsoft-megamenu/all-microsoft-megamenu.component';
import { NavMenuActivatorDirective } from './header/nav-menu-activator.directive';
import { NavMenuPanelDirective } from './header/nav-menu-panel.directive';
import { NavMenuItemComponent } from './header/nav-menu-item/nav-menu-item.component';

import { ErrorInterceptor } from './interceptors/error.interceptor';

function throwIfAlreadyLoaded(parentModule: any, moduleName: string) {
    if (parentModule) {
        throw new Error(`${moduleName} has already been loaded. Import Core modules in the AppModule only.`);
    }
}

@NgModule({
    exports: [
        FooterComponent,
        HeaderComponent,
    ],
    declarations: [
        FooterComponent,
        HeaderComponent,
        NavMenuItemComponent,
        AllMicrosoftMegamenuComponent,
        NavMenuActivatorDirective,
        NavMenuPanelDirective,
    ],
    imports: [
        CommonModule,
        HttpClientModule,
        RouterModule,

        SlbButtonModule,
        SlbDateFormatModule,
    ],
    providers: [
        { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    ]
})
export class CoreModule {
    constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
        throwIfAlreadyLoaded(parentModule, 'CoreModule');
    }
}
