import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(
            catchError(err => {
                if (err instanceof HttpErrorResponse) {
                    let message = 'An unknown error has occurred';

                    if (err.error !== null) {
                        if (err.error.message) {
                            // Extract the error information out of the response
                            message = err.error.message;
                        } else if (err.error.title && err.error.errors) {
                            // Model validation error
                            message = err.error.title;
                        }
                    } else if (err.status === 400) {
                        /// If server gives back 400 status code, then XSRF token broken
                        message = 'An unexpected error occurred, please try refreshing the page.';
                    }

                    return throwError({
                        message
                    });
                }
                return throwError(err);
            })
        );
    }
}
