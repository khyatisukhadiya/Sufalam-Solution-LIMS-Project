import { CommonModule } from '@angular/common';
import { Component, ElementRef, Inject, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { StateService } from '../../service/state/state.service';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { stateModal } from '../../modal/stateModal';
import { Modal } from 'bootstrap';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";


@Component({
  selector: 'app-state',  // Updated component selector
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './state.component.html',
  styleUrl: './state.component.css'
})
export class StateComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;

  stateForm: FormGroup = new FormGroup({});

  stateService = inject(StateService);

  filteredStates: any = [];
  states: any[] = [];
  modalInstance: Modal | undefined;
  searchClick: boolean = false;
  submitted: boolean = false;
  stateList: stateModal[] = [];
  isEditModal = false;
  searchCriteria = { stateId: '', stateCode: '', stateName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';
  countries: any[] = [];
  selectedCountryName: string = '';

  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.setForm();
    this.loadCountries();
  }

  loadCountries() {
    console.log('Fetching countries from API...');
    this.stateService.GetCountry().subscribe({
      next: (res) => {
        console.log('Countries loaded:', res);  // This should log the countries array
        this.countries = res || [];
      },
      error: (err) => {
        console.error('Error loading countries:', err);
      }
    });
  }


  openModal() {
    const stateModal = document.getElementById('myModal');
    if (stateModal != null) {
      stateModal.style.display = "block";
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

    if (this.modal != null) {
      this.modal.nativeElement.style.display = "none";
    }

    if (this.modalInstance) {
      this.modalInstance.hide();
    }


    this.errorMessage = '';
    this.validationErrors = [];
    this.submitted = false;
    
    this.stateForm.reset({
      StateId: 0,
      StateName: '',
      StateCode: '',
      countryId: null,
      isActive: true
    });

  }

  // toaster message
  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

  // input change to remove error
  onInputChange(): void {
    Object.keys(this.stateForm.controls).forEach((field) => {
      const control = this.stateForm.get(field);
      if (control && control.errors) {
        control.setErrors(null);
      }
    });

    this.validationErrors = [];
    this.errorMessage = '';
  }

  onCountryChange(selectedCountryId: number): void {
    this.stateForm.get('countryID')?.setValue(selectedCountryId);
    this.onInputChange(); // clear errors when country changes
  }

  // Add/Edit Form submit
  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    if (this.stateForm.invalid) {
      return;
    }

    // Get raw value (including disabled fields) from the form
    const payload = this.stateForm.getRawValue();

    console.log(payload);

    const isUpdate = payload.StateId && payload.StateId > 0;
    this.stateService.addUpdatedState(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.getState();
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
    this.stateForm = this.fb.group({
      StateId: [{ value: 0, disabled: true }],
      StateName: ['', Validators.required],
      StateCode: ['', Validators.required],
      isActive: [true],
      countryId: [null, Validators.required]
    });
  }

  // all states is fetch
  getState(): void {
    this.searchClick = true;
  
    const filter = {
      name: this.searchCriteria.stateName?.trim() || '',
      code: this.searchCriteria.stateCode?.trim() || '',
      id: this.searchCriteria.stateId || null,
      isActive: this.searchCriteria.isActive,
    };
  
    if (!this.countries.length) {
      console.log('Countries data not loaded yet');
      return;
    }
  
    this.stateService.fetchState(filter).subscribe({
      next: (res) => {
        console.log('Raw API response:', res);
        this.stateList = res?.data || [];
      },
      error: (err) => {
        console.error('Failed to load states:', err);
      }
    });
  }
  


  // Edit Modal
  onEdit(StateId : number) {

    this.stateService.GetStateById(StateId).subscribe(response => {

        console.log('onedit',response);

        this.stateForm.patchValue({
          StateId : response.stateId,
          StateCode : response.stateCode,
          StateName : response.stateName,
          countryId : response.countryId,
          countryName : response.countryName,
          isActive : response.isActive
        });

        
            // const country = this.countries.find(c => c.countryId === response.countryId);
            // const countryName = country ? country.countryName : 'Unknown';
            // this.selectedCountryName = countryName;
        
            this.isEditModal = true;
            this.openModal();
    });
  }


  // delete means active to inactive and reverse
  onDelete(stateId: number) {
    const state = this.stateList.find(s => s.stateId === stateId);
    if (state && state.isActive === null) {
      state.isActive = false;
    }

    this.stateService.deleteState(stateId).subscribe(() => {
      const message = state?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getState();
      this.closeModal();
    })
  }

  // clear search field
  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      stateId: '',
      stateName: '',
      stateCode: '',
      isActive: true
    };
  }
}
