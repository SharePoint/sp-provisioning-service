import { DisplayInfo } from './display-info.model';

export type PackageType = 'SiteCollection' | 'Tenant';

export class Package {
    id: string;
    abstract: string;
    displayName: string;
    description: string;
    packageType: PackageType;
    displayInfo: DisplayInfo;
    provisioningFormUrl: string;
}
