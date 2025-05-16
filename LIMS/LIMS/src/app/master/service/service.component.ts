import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ServiceService } from '../../service/MasterService/service/service.service';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Modal } from 'bootstrap';
import { serviceModal } from '../../modal/MasterModel/serviceModal';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";


@Component({
  selector: 'app-service',
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './service.component.html',
  styleUrls: ['./service.component.css']
})
export class ServiceComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
@ViewChild('autofocus') autofocus!: ElementRef;

  serviceForm: FormGroup = new FormGroup({});
  serviceService = inject(ServiceService)
  filteredServices: any = [];
  modalInstance: Modal | undefined;
  services: any[] = []
  searchClick: boolean = false;
  submitted: boolean = false;
  serviceList: serviceModal[] = [];
  isEditModal = false;
  searchCriteria = { serviceId: '', serviceCode: '', serviceName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';
  selectedTab: string = 'general';
  selectedTests: any[] = [];
  tests: any[] = [];

  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.setForm();
    this.loadTests();
  }

  loadTests() {
    console.log('Fetching countries from API...');
    this.serviceService.GetTests().subscribe({
      next: (res) => {
        this.tests = res || [];
      },
      error: (err) => {
        console.error('Error loading states:', err);
      }
    });
  }


  openModal() {
    // this.isEditModal = false;
    const modalEl = document.getElementById('myModal');

    if (modalEl != null) {
      modalEl.style.display = "block";
      modalEl.addEventListener('shown.bs.modal', () => {
        if (this.autofocus) {
          this.autofocus.nativeElement.focus();
        }
      }); 
    }
    this.selectedTab = 'general';

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
    (document.activeElement as HTMLElement)?.blur(); // Fix aria-hidden warning

    if (this.modalInstance) {
      this.modalInstance.hide();
    }

    this.errorMessage = '';
    this.validationErrors = [];
    this.submitted = false;
    this.selectedTests = [];

    this.serviceForm.reset({
      ServiceId: 0,
      ServiceName: '',
      ServiceCode: '',
      B2BAmount: 0,
      B2CAmount: 0,
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
    Object.keys(this.serviceForm.controls).forEach((field) => {
      const control = this.serviceForm.get(field);
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

    if (this.serviceForm.invalid) {
      return;
    }

    const formValues = this.serviceForm.getRawValue();

    const payload = {
      service: {
        serviceId: formValues.ServiceId,
        serviceName: formValues.ServiceName,
        serviceCode: formValues.ServiceCode,
        b2BAmount: formValues.B2BAmount,
        b2CAmount: formValues.B2CAmount,
        isActive: formValues.isActive
      },
      tests: this.selectedTests 
    };


    this.serviceService.addUpdatedService(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
          this.getService();
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
    this.serviceForm = this.fb.group({
      ServiceId: [{ value: 0, disabled: true }],
      ServiceName: ['', Validators.required],
      ServiceCode: ['', Validators.required],
      B2BAmount: [0, Validators.required],
      B2CAmount: [0, Validators.required],
      testId: [''],
      isActive: [true],
    });
  }

  getService(): void {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.serviceName?.trim() || '',
      code: this.searchCriteria.serviceCode?.trim() || '',
      id: this.searchCriteria.serviceId || null,
      isActive: this.searchCriteria.isActive
    };

    this.serviceService.fetchService(filter).subscribe({
      next: (res) => {
        this.serviceList = res?.data || [];
      },
      error: (err) => {
        console.error('Failed to load services:', err);
      }
    });
  }

  onEdit(serviceId: number) {
    this.serviceService.getServiceById(serviceId).subscribe({
      next: (res) => {
        this.serviceForm.patchValue({
          ServiceId: res.serviceId,
          ServiceName: res.serviceName,
          ServiceCode: res.serviceCode,
          B2BAmount: res.b2BAmount,
          B2CAmount: res.b2CAmount,
          isActive: res.isActive,
          testId: null
        });
        this.selectedTests = res.test || [];
        this.isEditModal = true;
        this.openModal();
        this.getService();
      }
    });
  }

  onDelete(serviceId: number) {
    const service = this.serviceList.find(s => s.serviceId === serviceId);
    if (service && service.isActive === null) {
      service.isActive = false;
    }

    this.serviceService.deleteService(serviceId).subscribe(() => {
      const message = service?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getService();
      this.closeModal();
    });
  }


  // onDeleteSetviceTestById(ServiceTestId : number){
  //   this.serviceService.deleteServiceTestId(ServiceTestId).subscribe(() =>
  //     {
  //       const message = 'Service Test Deleted';
  //       this.showSuccess(message);
  //       this.getService();
  //       });
  // }

  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      serviceId: '',
      serviceName: '',
      serviceCode: '',
      isActive: true
    };
  }


  removeTest(test: any): void {
  this.serviceService.deleteServiceTestId(test.serviceTestId).subscribe(() => {
    this.selectedTests = this.selectedTests.filter(t => t.serviceTestId !== test.serviceTestId);
    const message = 'Service Test Deleted';
    this.showSuccess(message);
    this.getService();
  });
}



  addTest() {
    const selectedTestId = this.serviceForm.value.testId;
    const selectedTest = this.tests.find(t => t.testId === selectedTestId);

    if (!selectedTest) return;

    const alreadyExists = this.selectedTests.some(test => test.testId === selectedTest.testId);
    if (!alreadyExists) {
      this.selectedTests.push(selectedTest);
    } else {
        this.showError("This is already added.");
    }

    // Reset the testId field in the form after adding the test
    this.serviceForm.patchValue({ testId: null });
  }




  onTestChange(event: any): void {
    const selectedTest = event.target.value;
  }


}
