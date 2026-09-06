

// import { ComponentFixture, TestBed } from '@angular/core/testing';

// import { EnrollmentList } from './enrollment-list';

// describe('EnrollmentList', () => {
//   let component: EnrollmentList;
//   let fixture: ComponentFixture<EnrollmentList>;

//   beforeEach(async () => {
//     await TestBed.configureTestingModule({
//       imports: [EnrollmentList],
//     }).compileComponents();

//     fixture = TestBed.createComponent(EnrollmentList);
//     component = fixture.componentInstance;
//     await fixture.whenStable();
//   });

//   it('should create', () => {
//     expect(component).toBeTruthy();
//   });
// });

import { ComponentFixture, TestBed } from "@angular/core/testing";

import { EnrollmentListComponent } from "./enrollment-list.component";

describe("EnrollmentListComponent", () => {
  let component: EnrollmentListComponent;
  let fixture: ComponentFixture<EnrollmentListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EnrollmentListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EnrollmentListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});


