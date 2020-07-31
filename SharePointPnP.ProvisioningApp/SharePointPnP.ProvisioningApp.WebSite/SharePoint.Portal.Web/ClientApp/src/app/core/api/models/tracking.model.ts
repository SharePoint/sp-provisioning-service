export interface Tracking {
    SourceId: string;
    SourceTrackingAction: '0';
    SourceTrackingUrl?: string;
    TemplateId?: string;
    SourceTrackingFromProduction: 'false' | 'true'
}