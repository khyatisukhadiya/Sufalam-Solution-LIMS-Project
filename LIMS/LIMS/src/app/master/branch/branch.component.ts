import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { BranchService } from '../../service/branch/branch.service';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Modal } from 'bootstrap';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { branchModal } from '../../modal/branchModal';

@Component({
  selector: 'app-branch',
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './branch.component.html',
  styleUrl: './branch.component.css'
})
export class BranchComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
  branchForm: FormGroup = new FormGroup({});
  branchService = inject(BranchService)
  filteredBranches: any = [];
  modalInstance: Modal | undefined;
  branches: any[] = []
  searchClick: boolean = false;
  submitted: boolean = false;
  branchList: branchModal[] = [];
  isEditModal = false;
  searchCriteria = { branchId: '', branchCode: '', branchName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';

  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.setForm();
  }

  openModal() {
    const branchModal = document.getElementById('myModal');
    
    if (branchModal != null) {
      branchModal.style.display = "block";
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

    this.branchForm.reset({
      BranchId: 0,
      BranchName: '',
      BranchCode: '',
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
    Object.keys(this.branchForm.controls).forEach((field) => {
      const control = this.branchForm.get(field);
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

    if (this.branchForm.invalid) {
      return;
    }

    const payload = this.branchForm.getRawValue();
    const isUpdate = payload.BranchId && payload.BranchId > 0;

    this.branchService.addUpdatedBranch(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
          this.getBranch();
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
    this.branchForm = this.fb.group({
      BranchId: [{ value: 0, disabled: true }],
      BranchName: ['', Validators.required],
      BranchCode: ['', Validators.required],
      isActive: [true],
    });
  }

  getBranch(): void {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.branchName?.trim() || '',
      code: this.searchCriteria.branchCode?.trim() || '',
      id: this.searchCriteria.branchId || null,
      isActive: this.searchCriteria.isActive
    };

    this.branchService.fetchBranch(filter).subscribe({
      next: (res) => {
        this.branchList = res?.data || [];
      },
      error: (err) => {
        console.error('Failed to load branches:', err);
      }
    });
  }

  onEdit(branchId: number) {
    this.branchService.getBranchById(branchId).subscribe({
      next: (res) => {
        this.branchForm.patchValue({
          BranchId: res.branchId,
          BranchName: res.branchName,
          BranchCode: res.branchCode,
          isActive: res.isActive
        });
        this.isEditModal = true;
        this.openModal();
        this.getBranch();
      }
    });
  }

  onDelete(branchId: number) {
    const branch = this.branchList.find(b => b.branchId === branchId);
    if (branch && branch.isActive === null) {
      branch.isActive = false;
    }

    this.branchService.deleteBranch(branchId).subscribe(() => {
      const message = branch?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getBranch();
      this.closeModal();
    });
  }

  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      branchId: '',
      branchName: '',
      branchCode: '',
      isActive: true
    };
  }
}
