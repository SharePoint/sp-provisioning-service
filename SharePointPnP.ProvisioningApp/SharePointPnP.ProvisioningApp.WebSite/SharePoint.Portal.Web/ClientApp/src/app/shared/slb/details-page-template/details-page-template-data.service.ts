import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';

import { DetailItemCategory, SystemRequirement, PackageType, PreviewImage } from 'src/app/core/api/models';

export class DetailsPageTemplateData {
    /**
     * Id of the package
     */
    packageId: string;

    /**
     * Title of the package
     */
    packageTitle: string;

    /**
     * Descriptor text of the package
     */
    packageDescriptor: string;

    /**
     * The type of the package
     */
    packageType: PackageType;

    /**
     * Paragraphs describing the package
     */
    packageDescription: string[];

    /**
     * The preview images for the package
     */
    previewImages: PreviewImage[];

    /**
     * Lists of detail items grouped by categories
     */
    detailItemCategories: DetailItemCategory[];

    /**
     * The requirements for the package
     */
    systemRequirements: SystemRequirement[];

    /**
     * The url for the provisioning form
     */
    provisioningFormUrl: string;

    /**
     * The url to use for hidden image in order to track the visit
     */
    telemetryUrl: string;
}

@Injectable()
export class DetailsPageTemplateDataService {
    get dataSnapshot(): DetailsPageTemplateData {
        return this.snapshot;
    }
    readonly data = new ReplaySubject<DetailsPageTemplateData>(1);
    private snapshot;

    constructor() { }

    /**
     * Set the details page template data
     * @param data
     */
    setDetailData(data: DetailsPageTemplateData) {
        this.snapshot = data;
        this.data.next(data);
    }
}
