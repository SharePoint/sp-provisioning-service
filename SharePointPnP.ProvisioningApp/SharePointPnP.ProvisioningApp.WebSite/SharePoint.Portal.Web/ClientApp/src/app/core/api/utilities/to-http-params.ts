import { HttpParams } from '@angular/common/http';

export function toHttpParams(obj: any): HttpParams {
    let httpParams = new HttpParams();
    if (obj) {
        Object.keys(obj)
            .filter(key => obj[key] !== null && obj[key] !== undefined)
            .forEach(key => {
                if (Array.isArray(obj[key])) {
                    obj[key].forEach(element => httpParams = httpParams.append(key, element));
                } else {
                    httpParams = httpParams.set(key, obj[key]);
                }
            });
    }
    return httpParams;
}
