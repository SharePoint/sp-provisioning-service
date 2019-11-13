import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AddToTenantButtonDirective } from './add-to-tenant-button.directive';
import { DetailsTemplateContextDirective } from './details-page-template-context.directive';
import { DetailItemComponent } from './detail-item/detail-item.component';
import { DisplayForCategoryDirective } from './display-for-category.directive';
import { PackageTitleComponent } from './package-title/package-title.component';
import { PackageDescriptionComponent } from './package-description/package-description.component';
import { PermissionMessageComponent } from './permission-message/permission-message.component';
import { SiteDescriptorComponent } from './site-descriptor/site-descriptor.component';
import { SystemRequirementsTableComponent } from './system-requirements-table/system-requirements-table.component';
import { TemplatePreviewImageDirective } from './template-preview-image.directive';

@NgModule({
    exports: [
        AddToTenantButtonDirective,
        DetailsTemplateContextDirective,
        DetailItemComponent,
        DisplayForCategoryDirective,
        PackageTitleComponent,
        PackageDescriptionComponent,
        PermissionMessageComponent,
        SiteDescriptorComponent,
        SystemRequirementsTableComponent,
        TemplatePreviewImageDirective,
    ],
    declarations: [
        AddToTenantButtonDirective,
        DetailsTemplateContextDirective,
        DetailItemComponent,
        DisplayForCategoryDirective,
        PackageTitleComponent,
        PackageDescriptionComponent,
        PermissionMessageComponent,
        SiteDescriptorComponent,
        SystemRequirementsTableComponent,
        TemplatePreviewImageDirective,
    ],
    imports: [
        CommonModule
    ]
})
export class SlbDetailsPageTemplateModule { }
