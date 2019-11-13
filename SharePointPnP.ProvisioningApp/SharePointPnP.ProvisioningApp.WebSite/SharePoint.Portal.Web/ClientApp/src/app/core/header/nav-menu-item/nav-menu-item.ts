export class NavMenuItem {
    name: string;
    subItems?: NavMenuItem[];
    routerLink?: any[];
    externalLink?: string;
    opensInNewTab?: boolean;
}
