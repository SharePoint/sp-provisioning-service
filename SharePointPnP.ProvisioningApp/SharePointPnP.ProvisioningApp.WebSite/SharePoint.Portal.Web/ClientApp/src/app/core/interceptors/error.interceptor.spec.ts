import { TestBed } from '@angular/core/testing';
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { Type } from '@angular/core';
import { HttpTestingController, HttpClientTestingModule } from '@angular/common/http/testing';

import { ErrorInterceptor } from './error.interceptor';

describe('ErrorInterceptor', () => {
    let httpMock: HttpTestingController;
    let http: HttpClient;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
            ],
        });
    });

    afterEach(() => httpMock.verify());

    it('should handle an http error with empty message', () => {
        http = TestBed.get(HttpClient);
        httpMock = TestBed.get(HttpTestingController as Type<HttpTestingController>);

        http.get('')
            .subscribe(
                () => { },
                error => {
                    expect(error).toEqual({
                        message: 'An unknown error has occurred'
                    });
                }
            );

        const req = httpMock.expectOne('');
        req.flush({ error: {} }, { status: 400, statusText: 'Error' });
    });

    it('should handle an http error with message', () => {
        http = TestBed.get(HttpClient);
        httpMock = TestBed.get(HttpTestingController as Type<HttpTestingController>);

        http.get('')
            .subscribe(
                () => { },
                error => {
                    expect(error).toEqual({
                        message: 'Test Error!'
                    });
                }
            );

        const req = httpMock.expectOne('');
        req.flush({ message: 'Test Error!', apiErrorCode: 'TestErrorCode' }, { status: 400, statusText: 'Error' });
    });

    it('should handle an http error with no message', () => {
        http = TestBed.get(HttpClient);
        httpMock = TestBed.get(HttpTestingController as Type<HttpTestingController>);

        http.get('')
            .subscribe(
                () => { },
                error => {
                    expect(error).toEqual({
                        message: 'An unexpected error occurred, please try refreshing the page.'
                    });
                }
            );

        const req = httpMock.expectOne('');
        req.flush(null, { status: 400, statusText: 'Error' });
    });
});
