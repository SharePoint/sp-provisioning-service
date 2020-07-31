import { BrowserModule, Title } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSnackBarModule } from '@angular/material';

import { CoreModule } from './core/core.module';
import { DynamicTemplateModule } from './shared/dynamic-template/dynamic-template.module';
import { PackageCardModule } from './shared/package-card/package-card.module';

import { SlbButtonModule } from './shared/slb/button/button.module';
import { SlbCarouselModule } from './shared/slb/carousel/carousel.module';
import { SlbExpandablePanelModule } from './shared/slb/expandable-panel/expandable-panel.module';
import { SlbTabsModule } from './shared/slb/tabs/tabs.module';
import { SlbStickyHeaderModule } from './shared/slb/sticky-header/sticky-header.module';
import { SlbDetailsPageTemplateModule } from './shared/slb/details-page-template/details-page-template.module';

import { AppRouterModule } from './app-router.module';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { DetailsComponent } from './details/details.component';
import { ServiceDescriptionComponent } from './service-description/service-description.component';

@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        DetailsComponent,
        ServiceDescriptionComponent
    ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        BrowserAnimationsModule,
        CoreModule,
        FormsModule,
        BrowserAnimationsModule,

        MatSnackBarModule,

        SlbButtonModule,
        SlbCarouselModule,
        SlbExpandablePanelModule,
        SlbTabsModule,
        SlbStickyHeaderModule,
        SlbDetailsPageTemplateModule,

        DynamicTemplateModule,
        PackageCardModule,

        AppRouterModule,
    ],
    providers: [
        Title
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
