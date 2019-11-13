import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TabLabelComponent } from './tab-label.component';

describe('TabLabelComponent', () => {
  let component: TabLabelComponent;
  let fixture: ComponentFixture<TabLabelComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TabLabelComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TabLabelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
