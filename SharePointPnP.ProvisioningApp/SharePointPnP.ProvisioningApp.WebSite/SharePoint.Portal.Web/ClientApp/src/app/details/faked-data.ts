import { SystemRequirement } from '../core/api/models';
import { DetailItemCategory } from '../core/api/models/detail-item-category.model';

interface FakeData {
    requiresTenantAdmin: boolean;
    detailItemCategories: DetailItemCategory[];
    systemRequirements: SystemRequirement[];
}

export const fakedData: FakeData = {
    requiresTenantAdmin: true,
    detailItemCategories: [
        {
            name: 'Webparts',
            items: [
                {
                    name: 'Events',
                    description: 'Compact layout',
                    url: 'https://support.office.com/en-us/article/use-the-events-web-part-5fe4da93-5fa9-4695-b1ee-b0ae4c981909'
                },
                {
                    name: 'Group Calendar',
                    description: 'Office 365 group calendar',
                    url: 'https://support.office.com/en-us/article/use-the-group-calendar-web-part-eaf3c04d-5699-48cb-8b5e-3caa887d51ce'
                },
                {
                    name: 'Highlighted Content',
                    description: 'Sourced library, card view',
                    // tslint:disable-next-line: max-line-length
                    url: 'https://support.office.com/en-us/article/use-the-highlighted-content-web-part-e34199b0-ff1a-47fb-8f4d-dbcaed329efd'
                },
                {
                    name: 'Instagram',
                    badgeText: 'Custom Web Part'
                }, {
                    name: 'News',
                    description: 'Carousel, top story layouts',
                    // tslint:disable-next-line: max-line-length
                    url: 'https://support.office.com/en-us/article/use-the-news-web-part-on-a-sharepoint-page-c2dcee50-f5d7-434b-8cb9-a7feefd9f165'
                }
            ]
        },
        {
            name: 'Features',
            items: [
                {
                    name: 'Header background'
                },
                {
                    name: 'Section background'
                },
                {
                    name: 'Footer',
                    // tslint:disable-next-line: max-line-length
                    url: 'https://support.office.com/en-us/article/change-the-look-of-your-sharepoint-site-06bbadc3-6b04-4a60-9d14-894f6a170818'
                },
                {
                    name: 'Custom logo',
                    badgeText: 'Coming Soon'
                }
            ]
        },
        {
            name: 'Site Content',
            items: [
                {
                    name: 'New site collection using communication site template, unless applied on top of existing site'
                },
                {
                    name: 'Custom welcome page'
                },
                {
                    name: '7 news articles with example content'
                },
                {
                    name: 'Sample image content used in the template'
                }
            ]
        }
    ],
    systemRequirements: [
        { name: 'OS', value: 'SharePoint' },
        { name: 'Permission Required', value: 'Tenant Admin' },
        { name: 'Site Language', value: 'English' },
    ]
};
