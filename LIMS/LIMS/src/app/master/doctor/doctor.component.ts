import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { DoctorService } from '../../service/doctor/doctor.service';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Modal } from 'bootstrap';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { doctorModal } from '../../modal/doctorModal';

@Component({
  selector: 'app-doctor',
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './doctor.component.html',
  styleUrl: './doctor.component.css'
})
export class DoctorComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
  doctorForm: FormGroup = new FormGroup({});
  doctorService = inject(DoctorService)
  filteredDoctors: any = [];
  modalInstance: Modal | undefined;
  doctors: any[] = []
  searchClick: boolean = false;
  submitted: boolean = false;
  doctorList: doctorModal[] = [];
  isEditModal = false;
  searchCriteria = { doctorId: '', doctorCode: '', doctorName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';

  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.setForm();
  }

  openModal() {
    const doctorModal = document.getElementById('myModal');

    if (doctorModal != null) {
      doctorModal.style.display = "block";
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

    this.doctorForm.reset({
      DoctorId: 0,
      DoctorName: '',
      DoctorCode: '',
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
    Object.keys(this.doctorForm.controls).forEach((field) => {
      const control = this.doctorForm.get(field);
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

    if (this.doctorForm.invalid) {
      return;
    }

    const payload = this.doctorForm.getRawValue();
    const isUpdate = payload.DoctorId && payload.DoctorId > 0;

    this.doctorService.addUpdatedDoctor(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
          this.getDoctor();
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
    this.doctorForm = this.fb.group({
      DoctorId: [{ value: 0, disabled: true }],
      DoctorName: ['', Validators.required],
      DoctorCode: ['', Validators.required],
      Email : ['', Validators.required],
      PhoneNumber : ['',Validators.required],
      isActive: [true],
    });
  }

  getDoctor(): void {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.doctorName?.trim() || '',
      code: this.searchCriteria.doctorCode?.trim() || '',
      id: this.searchCriteria.doctorId || null,
      isActive: this.searchCriteria.isActive
    };

    this.doctorService.fetchDoctor(filter).subscribe({
      next: (res) => {
        this.doctorList = res?.data || [];
      },
      error: (err) => {
        console.error('Failed to load doctors:', err);
      }
    });
  }

  onEdit(doctorId: number) {
    this.doctorService.getDoctorById(doctorId).subscribe({
      next: (res) => {
        this.doctorForm.patchValue({
          DoctorId: res.doctorId,
          DoctorName: res.doctorName,
          DoctorCode: res.doctorCode,
          Email : res.email,
          PhoneNumber : res.phoneNumber,
          isActive: res.isActive
        });
        this.isEditModal = true;
        this.openModal();
        this.getDoctor();
      }
    });
  }

  onDelete(doctorId: number) {
    const doctor = this.doctorList.find(d => d.doctorId === doctorId);
    if (doctor && doctor.isActive === null) {
      doctor.isActive = false;
    }

    this.doctorService.deleteDoctor(doctorId).subscribe(() => {
      const message = doctor?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getDoctor();
      this.closeModal();
    });
  }

  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      doctorId: '',
      doctorName: '',
      doctorCode: '',
      isActive: true
    };
  }
}
