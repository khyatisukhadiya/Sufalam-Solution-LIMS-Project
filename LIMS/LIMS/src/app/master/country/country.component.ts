import { CommonModule } from '@angular/common';
import { Component, ElementRef, Inject, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CountryService } from '../../service/MasterService/country/country.service';
import { RouterModule } from '@angular/router';
import { countryModal } from '../../modal/MasterModel/countrymodal';
import { ToastrService } from 'ngx-toastr';
import { filter } from 'rxjs';
import { Modal } from 'bootstrap';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";

@Component({
  selector: 'app-country',
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './country.component.html',
  styleUrl: './country.component.css'
})
export class CountryComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
@ViewChild('autofocus') autofocus!: ElementRef;

  countryForm: FormGroup = new FormGroup({});
  countryService = inject(CountryService)
  filteredCountries: any = [];
  modalInstance: Modal | undefined;
  countries: any[] = []
  searchClick: boolean = false;
  submitted: boolean = false;
  countryList: countryModal[] = [];
  isEditModal = false;
  searchCriteria = { countryId: '', countryCode: '', countryName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';

  constructor(private fb: FormBuilder, private toastr: ToastrService) { }


  ngOnInit(): void {
    this.setForm();
  }


  openModal() {
    const countryModal = document.getElementById('myModal');
    
    if (countryModal != null) {
      countryModal.style.display = "block";
      countryModal.addEventListener('shown.bs.modal', () => {
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

    this.countryForm.reset({
      CountryId: 0,
      CountryName: '',
      CountryCode: '',
      isActive: true
    });


  }

  // toster message
  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

  // input chnage to remove error
  onInputChange(): void {
    // Reset all form control errors
    Object.keys(this.countryForm.controls).forEach((field) => {
      const control = this.countryForm.get(field);
      if (control && control.errors) {
        control.setErrors(null);
      }
    });


    this.validationErrors = [];
    this.errorMessage = '';
  }



  // Add/Edit Form submit
  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    if (this.countryForm.invalid) {
      return;
    }

    const payload = this.countryForm.getRawValue();

    console.log(payload);

    const isUpdate = payload.CountryId && payload.CountryId > 0;


    this.countryService.addUpdatedCountry(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
          this.getCountry();
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
    this.countryForm = this.fb.group({
      CountryId: [{ value: 0, disabled: true }],
      CountryName: ['', Validators.required],
      CountryCode: ['', Validators.required],
      isActive: [true],
    });
  }


  // all country is fetch
  getCountry(): void {
    this.searchClick = true;
  
    const filter = {
      name: this.searchCriteria.countryName?.trim() || '',
      code: this.searchCriteria.countryCode?.trim() || '',
      id: this.searchCriteria.countryId || null,
      isActive: this.searchCriteria.isActive
    };
  
    this.countryService.fetchCountry(filter)
      .subscribe({
        next: (res) => {
          this.countryList = res?.data || [];
        },
        error: (err) => {
          console.error('Failed to load countries:', err);
        }
      });
  }
  

  // Edit Modal
  onEdit(countryId : number) {
    this.countryService.getCountryById(countryId).subscribe({
      next : (res) => {
        console.log('onedit', res);
        
        this.countryForm.patchValue({
          CountryId : res.countryId,
          CountryName : res.countryName,
          CountryCode : res.countryCode,
          isActive : res.isActive
        });
        this.isEditModal = true;
        this.openModal();
        this.getCountry();
      }
    }); 
  }
  


  // delete means active to inactive and reverse
  onDelete(countryId: number) {

    const country = this.countryList.find(c => c.countryId === countryId);
    if (country && country.isActive === null) {
      country.isActive = false;  
    }

    this.countryService.deleteCountry(countryId).subscribe(() => {
      const message = country?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getCountry();
      this.closeModal();
    })
  }

  // clear search field
  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      countryId: '',
      countryName: '',
      countryCode: '',
      isActive: true
    };
  }

}
