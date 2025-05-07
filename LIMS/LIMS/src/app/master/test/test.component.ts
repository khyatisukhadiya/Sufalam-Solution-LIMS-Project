import { CommonModule } from '@angular/common';
import { Component, ElementRef, Inject, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TestService } from '../../service/test/test.service';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { filter } from 'rxjs';
import { Modal } from 'bootstrap';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { testModal } from '../../modal/testModal';

@Component({
  selector: 'app-test',
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './test.component.html',
  styleUrl: './test.component.css'
})
export class TestComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
  testForm: FormGroup = new FormGroup({});
  testService = inject(TestService)
  filteredTests: any = [];
  modalInstance: Modal | undefined;
  tests: any[] = []
  searchClick: boolean = false;
  submitted: boolean = false;
  testList: testModal[] = [];
  isEditModal = false;
  searchCriteria = { testId: '', testCode: '', testName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';

  constructor(private fb: FormBuilder, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.setForm();
  }

  openModal() {
    const testModal = document.getElementById('myModal');

    if (testModal != null) {
      testModal.style.display = "block";
    }

    if (this.modal?.nativeElement) {
      this.modalInstance = new Modal(this.modal.nativeElement, {
        backdrop: 'static',
        keyboard: false
      });
      this.modalInstance.show();
    }
  }

  closeModal() {
    this.isEditModal = false;

    if (this.modalInstance) {
      this.modalInstance.hide();
    }

    if (this.modal != null) {
      this.modal.nativeElement.style.display = "none";
    }

    this.errorMessage = '';
    this.validationErrors = [];
    this.submitted = false;

    this.testForm.reset({
      TestId: 0,
      TestName: '',
      TestCode: '',
      isActive: true
    });
  }

  // toastr messages
  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

  // input change to remove validation errors
  onInputChange(): void {
    Object.keys(this.testForm.controls).forEach((field) => {
      const control = this.testForm.get(field);
      if (control && control.errors) {
        control.setErrors(null);
      }
    });

    this.validationErrors = [];
    this.errorMessage = '';
  }

  // Add/Edit Test form submit
  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    if (this.testForm.invalid) {
      return;
    }

    const payload = this.testForm.getRawValue();
    console.log(payload);

    const isUpdate = payload.TestId && payload.TestId > 0;

    this.testService.addUpdatedTest(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
          this.getTest();
          this.isEditModal = false;
        } else if (res.errors) {
          this.validationErrors = res.errors;
        }
      },
      error: (err) => {
        if (err.status === 400 && err.error?.errors) {
          this.validationErrors = err.error.errors;
        } else {
          this.errorMessage = 'An unexpected error occurred.';
          this.showError(this.errorMessage);
        }
      }
    });
  }

  setForm() {
    this.testForm = this.fb.group({
      TestId: [{ value: 0, disabled: true }],
      TestName: ['', Validators.required],
      TestCode: ['', Validators.required],
      isActive: [true],
    });
  }

  // fetch all test data
  getTest(): void {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.testName?.trim() || '',
      code: this.searchCriteria.testCode?.trim() || '',
      id: this.searchCriteria.testId || null,
      isActive: this.searchCriteria.isActive
    };

    this.testService.fetchTest(filter).subscribe({
      next: (res) => {
        this.testList = res?.data || [];
      },
      error: (err) => {
        console.error('Failed to load test data:', err);
      }
    });
  }

  // Edit modal logic
  onEdit(testId: number) {
    this.testService.getTestById(testId).subscribe({
      next: (res) => {
        console.log('onEdit', res);

        this.testForm.patchValue({
          TetsId: res.testId,
          TestName: res.testName,
          TestCode: res.testCode,
          isActive: res.isActive
        });
        this.isEditModal = true;
        this.openModal();
        this.getTest();
      }
    });
  }

  // toggle active/inactive status
  onDelete(testId: number) {
    const test = this.testList.find(c => c.testId === testId);
    if (test && test.isActive === null) {
      test.isActive = false;
    }

    this.testService.deleteTest(testId).subscribe(() => {
      const message = test?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getTest();
      this.closeModal();
    });
  }

  // clear search form
  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      testId: '',
      testName: '',
      testCode: '',
      isActive: true
    };
  }
}
