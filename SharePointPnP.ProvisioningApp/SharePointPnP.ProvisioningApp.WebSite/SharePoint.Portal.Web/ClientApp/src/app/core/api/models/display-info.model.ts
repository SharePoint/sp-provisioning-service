import { PreviewImage } from './preview-image.model';
import { DetailItemCategory } from './detail-item-category.model';
import { SystemRequirement } from './system-requirement.model';

export class DisplayInfo {
    pageTemplateId: string;
    siteDescriptor: string;
    descriptionParagraphs: string[];
    previewImages: PreviewImage[];
    detailItemCategories: DetailItemCategory[];
    systemRequirements: SystemRequirement[];
}
