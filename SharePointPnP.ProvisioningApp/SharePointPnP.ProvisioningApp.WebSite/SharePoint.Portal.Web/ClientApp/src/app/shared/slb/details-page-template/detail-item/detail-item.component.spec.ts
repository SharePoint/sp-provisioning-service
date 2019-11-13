import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { DetailItemComponent } from './detail-item.component';

describe('DetailItemComponent', () => {
    let component: DetailItemComponent;
    let fixture: ComponentFixture<DetailItemComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [DetailItemComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(DetailItemComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => expect(component).toBeTruthy());

    it('should display text only if there is no url', () => {
        const name = 'text item';
        component.item = { name };
        fixture.detectChanges();

        const linkElem = fixture.debugElement.query(By.css('a'));
        expect(linkElem).toBeFalsy();
        expect(fixture.debugElement.nativeElement.innerText).toEqual(name);
    });

    it('should display a link if there is a url', () => {
        const name = 'text item';
        const url = '/path';
        component.item = { name, url };
        fixture.detectChanges();

        const linkElem = fixture.debugElement.query(By.css('a'));
        expect(linkElem).toBeTruthy();
        expect(linkElem.nativeElement.attributes.href.value).toEqual(url);
    });
});
