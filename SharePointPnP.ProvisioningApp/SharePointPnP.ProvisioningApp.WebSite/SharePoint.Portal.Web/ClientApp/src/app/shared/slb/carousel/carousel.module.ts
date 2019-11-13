import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PortalModule } from '@angular/cdk/portal';

import { CarouselComponent } from './carousel.component';
import { CarouselItemComponent } from './carousel-item/carousel-item.component';
import { CarouselItemBodyComponent } from './carousel-item-body/carousel-item-body.component';

@NgModule({
    exports: [
        CarouselComponent,
        CarouselItemComponent
    ],
    declarations: [
        CarouselComponent,
        CarouselItemComponent,
        CarouselItemBodyComponent
    ],
    imports: [
        CommonModule,
        PortalModule
    ]
})
export class SlbCarouselModule { }
