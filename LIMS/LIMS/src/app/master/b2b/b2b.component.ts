import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { B2BService } from '../../service/MasterService/b2b/b2b.service';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Modal } from 'bootstrap';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { b2bModal } from '../../modal/MasterModel/b2bModal';


@Component({
  selector: 'app-b2b',
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './b2b.component.html',
  styleUrl: './b2b.component.css'
})
export class B2BComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
@ViewChild('autofocus') autofocus!: ElementRef;

  b2bForm: FormGroup = new FormGroup({});
  b2bService = inject(B2BService);
  filteredB2Bs: any = [];
  modalInstance: Modal | undefined;
  b2bs: any[] = [];
  searchClick: boolean = false;
  submitted: boolean = false;
  b2bList: b2bModal[] = [];
  isEditModal = false;
  searchCriteria = { b2BId: '', b2BCode: '', b2BName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';

  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.setForm();
  }

  openModal() {
    const b2bModal = document.getElementById('myModal');

    if (b2bModal != null) {
      b2bModal.style.display = "block";
      b2bModal.addEventListener('shown.bs.modal', () => {
        this.autofocus?.nativeElement.focus();
      }, { once: true });
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

    this.b2bForm.reset({
      b2BId: 0,
      b2BName: '',
      b2BCode: '',
      isActive: true
    });
  }

  // Toast messages
  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

  onInputChange(): void {
    Object.keys(this.b2bForm.controls).forEach((field) => {
      const control = this.b2bForm.get(field);
      if (control && control.errors) {
        control.setErrors(null);
      }
    });

    this.validationErrors = [];
    this.errorMessage = '';
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    if (this.b2bForm.invalid) {
      return;
    }

    const payload = this.b2bForm.getRawValue();
    const isUpdate = payload.b2BId && payload.b2BId > 0;

    this.b2bService.addUpdatedB2B(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
          this.getB2B();
          this.isEditModal = false;
        } 
        else if (res.errors) {
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
    this.b2bForm = this.fb.group({
      b2BId: [{ value: 0, disabled: true }],
      b2BName: ['', Validators.required],
      b2BCode: ['', Validators.required],
      isActive: [true],
    });
  }

  getB2B(): void {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.b2BName?.trim() || '',
      code: this.searchCriteria.b2BCode?.trim() || '',
      id: this.searchCriteria.b2BId || null,
      isActive: this.searchCriteria.isActive
    };

    this.b2bService.fetchB2B(filter).subscribe({
      next: (res) => {
        this.b2bList = res?.data || [];
      },
      error: (err) => {
        console.error('Failed to load B2Bs:', err);
      }
    });
  }

  onEdit(b2BId: number) {
    this.b2bService.getB2BById(b2BId).subscribe({
      next: (res) => {
        this.b2bForm.patchValue({
          b2BId: res.b2BId,
          b2BName: res.b2BName,
          b2BCode: res.b2BCode,
          isActive: res.isActive
        });
        this.isEditModal = true;
        this.openModal();
        this.getB2B();
      }
    });
  }

  onDelete(b2BId: number) {
    const b2B = this.b2bList.find(b => b.b2BId === b2BId);
    if (b2B && b2B.isActive === null) {
      b2B.isActive = false;
    }

    this.b2bService.deleteB2B(b2BId).subscribe(() => {
      const message = b2B?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getB2B();
      this.closeModal();
    });
  }

  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      b2BId: '',
      b2BName: '',
      b2BCode: '',
      isActive: true
    };
  }
}
