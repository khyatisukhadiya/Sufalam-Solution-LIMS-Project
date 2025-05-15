import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CityService } from '../../service/MasterService/city/city.service';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { cityModal } from '../../modal/MasterModel/cityModal';
import { Modal } from 'bootstrap';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";

@Component({
  selector: 'app-city',
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './city.component.html',
  styleUrl: './city.component.css'
})
export class CityComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;

  cityForm: FormGroup = new FormGroup({});
  cityService = inject(CityService);

  states: any[] = [];
  modalInstance: Modal | undefined;
  searchClick: boolean = false;
  submitted: boolean = false;
  cityList: cityModal[] = [];
  isEditModal = false;
  searchCriteria = { cityId: '', cityCode: '', cityName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';

  constructor(private fb: FormBuilder, private toastr: ToastrService) {}

  ngOnInit(): void {
    this.setForm();
    this.loadStates();
  }

  loadStates() {
    this.cityService.GetStates().subscribe({
      next: (res) => {
        this.states = res || [];
      },
      error: (err) => {
        console.error('Error loading states:', err);
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

    this.cityForm.reset({
      CityId: 0,
      CityName: '',
      CityCode: '',
      stateId: null,
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
    Object.keys(this.cityForm.controls).forEach((field) => {
      const control = this.cityForm.get(field);
      if (control && control.errors) {
        control.setErrors(null);
      }
    });

    this.validationErrors = [];
    this.errorMessage = '';
  }

  onStateChange(selectedStateId: number): void {
    this.cityForm.get('stateId')?.setValue(selectedStateId);
    this.onInputChange();
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    if (this.cityForm.invalid) {
      return;
    }

    const payload = this.cityForm.getRawValue();
    const isUpdate = payload.CityId && payload.CityId > 0;

    this.cityService.addUpdatedCity(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.getCity();
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
    this.cityForm = this.fb.group({
      CityId: [{ value: 0, disabled: true }],
      CityName: ['', Validators.required],
      CityCode: ['', Validators.required],
      isActive: [true],
      stateId: [null, Validators.required]
    });
  }

  getCity(): void {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.cityName?.trim() || '',
      code: this.searchCriteria.cityCode?.trim() || '',
      id: this.searchCriteria.cityId || null,
      isActive: this.searchCriteria.isActive
    };

    this.cityService.fetchCity(filter).subscribe({
      next: (res) => {
        this.cityList = res?.data || [];
      },
      error: (err) => {
        console.error('Failed to load cities:', err);
      }
    });
  }

  onEdit(cityId: number) {
    this.cityService.GetCityById(cityId).subscribe(response => {
      this.cityForm.patchValue({
        CityId: response.cityId,
        CityCode: response.cityCode,
        CityName: response.cityName,
        stateId: response.stateId,
        isActive: response.isActive
      });

      this.isEditModal = true;
      this.openModal();
    });
  }

  onDelete(cityId: number) {
    const city = this.cityList.find(c => c.cityId === cityId);
    if (city && city.isActive === null) {
      city.isActive = false;
    }

    this.cityService.deleteCity(cityId).subscribe(() => {
      const message = city?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getCity();
      this.closeModal();
    });
  }

  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      cityId: '',
      cityName: '',
      cityCode: '',
      isActive: true
    };
  }
}
