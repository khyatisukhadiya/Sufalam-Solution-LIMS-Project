import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AreaService } from '../../service/area/area.service';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Modal } from 'bootstrap';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { AreaModal } from '../../modal/areaModal';

@Component({
  selector: 'app-area',
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './area.component.html',
  styleUrl: './area.component.css'
})
export class AreaComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;

  areaForm: FormGroup = new FormGroup({});
  areaService = inject(AreaService);

  cities: any[] = [];
  modalInstance: Modal | undefined;
  searchClick: boolean = false;
  submitted: boolean = false;
  areaList: AreaModal[] = [];
  isEditModal = false;
  searchCriteria = { areaId: '', areaCode: '', areaName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';

  constructor(private fb: FormBuilder, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.setForm();
    this.loadCities();
  }

  loadCities() {
    this.areaService.getCities().subscribe({
      next: (res) => {
        this.cities = res || [];
      },
      error: (err) => {
        console.error('Error loading cities:', err);
      }
    });
  }

  openModal() {
    const modalEl = document.getElementById('myModal');
    if (modalEl) {
      modalEl.style.display = 'block';
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

    if (this.modal?.nativeElement) {
      this.modal.nativeElement.style.display = 'none';
    }

    if (this.modalInstance) {
      this.modalInstance.hide();
    }

    this.errorMessage = '';
    this.validationErrors = [];
    this.submitted = false;

    this.areaForm.reset({
      AreaId: 0,
      AreaName: '',
      AreaCode: '',
      cityId: null,
      isActive: true
    });
  }

  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

  onInputChange(): void {
    Object.keys(this.areaForm.controls).forEach((field) => {
      const control = this.areaForm.get(field);
      if (control && control.errors) {
        control.setErrors(null);
      }
    });

    this.validationErrors = [];
    this.errorMessage = '';
  }

  onCityChange(selectedCityId: number): void {
    this.areaForm.get('cityId')?.setValue(selectedCityId);
    this.onInputChange();
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    if (this.areaForm.invalid) {
      return;
    }

    const payload = this.areaForm.getRawValue();
    const isUpdate = payload.AreaId && payload.AreaId > 0;

    this.areaService.addUpdateArea(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.getAreas();
          this.isEditModal = false;
        } else if (res.errors) {
          this.validationErrors = res.errors || [];
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
    this.areaForm = this.fb.group({
      AreaId: [{ value: 0, disabled: true }],
      AreaName: ['', Validators.required],
      AreaCode: ['', Validators.required],
      isActive: [true],
      cityId: [null, Validators.required]
    });
  }

  getAreas(): void {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.areaName?.trim() || '',
      code: this.searchCriteria.areaCode?.trim() || '',
      id: this.searchCriteria.areaId || null,
      isActive: this.searchCriteria.isActive
    };

    this.areaService.fetchAreas(filter).subscribe({
      next: (res) => {
        this.areaList = res?.data || [];
      },
      error: (err) => {
        console.error('Failed to load areas:', err);
      }
    });
  }

  onEdit(areaId: number) {
    this.areaService.getAreaById(areaId).subscribe(response => {
      const area = response.data;

      this.areaForm.patchValue({
        AreaId: area.areaId,
        AreaCode: area.areaCode,
        AreaName: area.areaName,
        cityId: area.cityId,
        isActive: area.isActive
      });

      this.isEditModal = true;
      this.openModal();
    });
  }

  onDelete(areaId: number) {
    const area = this.areaList.find(a => a.areaId === areaId);
    if (area && area.isActive === null) {
      area.isActive = false;
    }

    this.areaService.deleteArea(areaId).subscribe(() => {
      const message = area?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getAreas();
      this.closeModal();
    });
  }

  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      areaId: '',
      areaName: '',
      areaCode: '',
      isActive: true
    };
  }
}
