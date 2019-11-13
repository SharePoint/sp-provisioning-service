import { Observable, of } from 'rxjs';

export interface ResolvedData<T> {
    /**
     * The resolved data. This is only set if the resolution was successful.
     */
    data?: T;

    /**
     * Error message from resolving the data. This is only set if an error occurred.
     */
    errorMessage?: string;
}

export function resolveError(errorMessage: string): Observable<ResolvedData<any>> {
    return of({ errorMessage });
}
