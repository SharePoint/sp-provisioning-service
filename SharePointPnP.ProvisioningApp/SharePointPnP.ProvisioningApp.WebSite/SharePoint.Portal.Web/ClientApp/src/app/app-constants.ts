export const appConstants = {
    getSiteTitle: (targetPlatformId: string) => targetPlatformId === 'LOOKBOOK'
        ? 'SharePoint look book' : 'SharePoint provisioning service',
    mediaBreakpointUp: {
        sm: '(min-width: 576px)',
        md: '(min-width: 768px)',
        lg: '(min-width: 992px)',
        xl: '(min-width: 1200px)'
    }
};
