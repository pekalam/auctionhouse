import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ImgUploadInputComponent } from './img-upload-input.component';

describe('ImgUploadInputComponent', () => {
  let component: ImgUploadInputComponent;
  let fixture: ComponentFixture<ImgUploadInputComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ImgUploadInputComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ImgUploadInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
