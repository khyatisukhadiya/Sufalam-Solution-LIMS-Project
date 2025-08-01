import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { SliderbarComponent } from '../../component/sliderbar/sliderbar.component';
import { RouterModule } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TestresultService } from '../../service/TransactionService/testresult/testresult.service';
import { TestService } from '../../service/MasterService/test/test.service';
import { testModal } from '../../modal/MasterModel/testModal';
import { serviceModal } from '../../modal/MasterModel/serviceModal';
import { Modal } from 'bootstrap';
import { SampleRegister } from '../../modal/Transaction/sampleRegister';
import { ToastrService } from 'ngx-toastr';
import { testresult } from '../../modal/Transaction/testresult';

@Component({
  selector: 'app-testresult',
  imports: [CommonModule, SliderbarComponent, RouterModule, FormsModule, ReactiveFormsModule],
  templateUrl: './testresult.component.html',
  styleUrl: './testresult.component.css'
})
export class TestresultComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
  @ViewChild('autofocus') autofocus: ElementRef | undefined;
  searchCriteria = { id: '', name: '', code: '' }
  testresultForm: FormGroup = new FormGroup({});
  testresultList: testresult[] = [];
  selectedSample: any = null;
  services: serviceModal[] = [];
  filteredServices: serviceModal[] = [];
  serviceNameFilter: string = '';
  filteredTests: any[] = [];
  selectedTest: any[] = [];
  submitted: boolean = false;
  validationErrors: string[] = [];
  modalInstance: any;
  sampleRegisterList: SampleRegister[] = [];
  searchClick: boolean = false;
  selectedSampleRegisterId: string | null = null;
  sampleRegisterServices: any[] = [];
  selectedServices: any[] = [];
  errorMessage: string = '';
  user : any;
  testresultService = inject(TestresultService)


  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.setFrom();
    this.loadServices();


    const userData = localStorage.getItem('loginDetails');
    if (userData) {
      this.user = JSON.parse(userData);
      this.testresultForm.get('createdBy')?.setValue(this.user.userName); 
    }
  }

  loadServices() {
    this.testresultService.getServices().subscribe({
      next: (res) => {
        this.services = res || [];
      },
      error: (err) => {
        console.error('Error loading services:', err);
      }
    });
  }



  closeModal() {
    if (this.modalInstance) {
      this.modalInstance.hide();
    }

    if (this.modal != null) {
      this.modal.nativeElement.style.display = "none";
    }

    this.submitted = false;

    this.testresultForm.reset(
      {
        testResultId: 0,
        sampleRegisterId: null,
        serviceId: null,
        testId: null,
        resultValue: '',
        validationStatus: '',
        validateBy: '',
        isActive: true
        }
    );
  }

  getSampleregister() {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.name?.trim() || '',
      id: this.searchCriteria.id || null,
      phoneNumber: this.searchCriteria.code
    };

    this.testresultService.getSampleregister(filter).subscribe({
      next: (res) => {
        this.sampleRegisterList = res.data || [];
      },
    })
  }

  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      id: '',
      name: '',
      code: '',
    }
  }

  openModal(sampleRegisterId: number, event: Event) {
     const modal = document.getElementById('myModal');
    
    if (modal != null) {
      modal.style.display = "block";
      // modal.addEventListener('shown.bs.modal', () => {
      //   this.autofocus?.nativeElement.focus();
      // }, { once: true });
    }

      if (this.modal?.nativeElement) {
      this.modalInstance = new Modal(this.modal.nativeElement, {
        backdrop: 'static',
        keyboard: false
      });
      this.modalInstance.show();
    }

    event.preventDefault();
    this.selectSample(sampleRegisterId);
  }

  selectSample(sampleRegisterId: number): void {
    this.testresultService.getSampleRegisterById(sampleRegisterId).subscribe({
      next: (res) => {
        console.log('API response:', res);
        this.selectedSample = res || null;
        this.selectedServices = (this.selectedSample?.serviceMapping || []).map((service: any) => ({
          ...service,
          tests: this.services.find(s => s.serviceId === service.serviceId)?.test || []
        }));

        console.log('selectedSample:', this.selectedSample);
        console.log('selectedServices with tests:', this.selectedServices);
      },
    })
  }

  setFrom() {
    this.testresultForm = this.fb.group({
      testResultId : [{value : 0, disabled: true}],
      sampleRegisterId: [null, Validators.required],
      serviceId: [null, Validators.required],
      testId: [null, Validators.required],
      resultValue: ['', Validators.required],
      validationStatus: ['', Validators.required],
      createdBy: ['', Validators.required],
      validateBy: ['', Validators.required],
      isActive: [true, Validators.required]
    });
  }

  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

  getrowvalue() {
    const services = this.selectedServices.map(service => ({
      serviceId: service.serviceId,
      serviceName: service.serviceName,
     
      tests : service.tests.map((test: any) => ({
        testId: test.testId,
        testName: test.testName,
        resultValue: test.resultValue,
        validationStatus: test.validationStatus,
         createdBy : this.user.userName,
        validateBy: test.validateBy || '',
        isActive :  test.isActive || true
      }))
    }));

    const formValues = {
      sampleregister : [{
        sampleRegisterId: this.selectedSample?.sampleRegisterId || null,
         services
      }]
    };

    console.log('formValues after mapping', formValues);
    console.log("selectedServices after mapping", this.selectedServices); 
    console.log("selectedSampleRegisterId after mapping", this.selectedSample?.sampleRegisterId);
    console.log("testresultForm after mapping", this.testresultForm.value);
    console.log("testresultForm raw value", this.testresultForm.getRawValue());
    console.log("testresultForm value", this.testresultForm.value);

    return formValues;
  }
    

  onSubmit() {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    //  if (this.testresultForm.invalid) {
    //   this.showError('Please fill all testResult fields correctly.');
    //   return;
    // }


    if (!this.selectedServices || !Array.isArray(this.selectedServices) || this.selectedServices.length === 0) {
      this.showError('No services selected.');
      return;
    }

    if (!this.selectedSample || !this.selectedSample.sampleRegisterId) {
      this.showError('No sample selected.');
      return;
    }


    const formValues = this.getrowvalue();

    this.testresultService.addUpdatedTestResult(formValues).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
        } else if (res.errors) {
          this.validationErrors = res.errors;
        }


      },
      error: (err) => {
        console.error('API error:', err);
        if (err.status === 400 && err.error?.errors) {
          this.validationErrors = err.error.errors;
        } else {
          this.errorMessage = 'An unexpected error occurred.';
          this.showError(this.errorMessage);
        }
      }
    });
  }
} 
