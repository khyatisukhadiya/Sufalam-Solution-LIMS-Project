import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Modal } from 'bootstrap';
import { RouterModule } from '@angular/router';
import { CommonModule, formatDate } from '@angular/common';
import { SampleRegister } from '../../modal/Transaction/sampleRegister';
import { SampleRegisterService } from '../../service/TransactionService/sample-register.service';
import { AreaModal } from '../../modal/MasterModel/areaModal';
import { cityModal } from '../../modal/MasterModel/cityModal';
import { branchModal } from '../../modal/MasterModel/branchModal';
import { b2bModal } from '../../modal/MasterModel/b2bModal';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-sample-register',
  imports: [SliderbarComponent, RouterModule, CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './sample-register.component.html',
  styleUrl: './sample-register.component.css'
})
export class SampleRegisterComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
  sampleRegisterForm: FormGroup = new FormGroup({});
  isEditModal: boolean = false;
  modalInstance: any;
  searchClick: boolean = false;
  isB2B: boolean = false;
  submitted: boolean = false;
  sampleRegisterList: SampleRegister[] = [];
  cities: cityModal[] = [];
  areas: AreaModal[] = [];
  branches: branchModal[] = [];
  b2bs: b2bModal[] = [];
  services: any[] = [];
  payments: any[] = [];
  selectedServices: any[] = [];
  selectedPayment: string = '';
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';
  searchCriteria = { sampleRegisterId: '', name: '', isActive: true };


  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  sampleRegisterService = inject(SampleRegisterService);

  ngOnInit(): void {
    this.setForm();


    this.sampleRegisterForm.get('dOB')?.valueChanges.subscribe((dobValue: string) => {
      if (dobValue) {
        this.calculateAge(dobValue);
      } else {
        this.sampleRegisterForm.patchValue({ Age: 0 });
      }
    });

    this.sampleRegisterForm.get('isB2B')?.valueChanges.subscribe((value: boolean) => {
      this.isB2B = value;
      this.calculateTotalAmount();
    });


    this.sampleRegisterForm.get('CityId')?.valueChanges.subscribe(() => {
      this.loadAreas();
    });


    this.loadCities();
    this.loadAreas();
    this.loadBranches();
    this.loadB2Bs();
    this.loadServices();
    this.loadPayments();
  }

  openModal() {
    this.submitted = false;
    this.validationErrors = [];
    const modalEl = document.getElementById('myModal');
    if (modalEl != null) {
      modalEl.style.display = "block";
    }

    if (!this.isEditModal) {
      const today = new Date().toISOString().split('T')[0];
      this.sampleRegisterForm.patchValue({ Date: today });
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
    (document.activeElement as HTMLElement)?.blur();

    if (this.modalInstance) {
      this.modalInstance.hide();
    }

    if (this.modal != null) {
      this.modal.nativeElement.style.display = "none";
    }


    this.errorMessage = '';
    this.validationErrors = [];
    this.submitted = false;
    this.selectedServices = [];

    this.sampleRegisterForm.reset({
      SampleRegisterId: 0,
      date: '',
      BranchId: null,
      AreaId: null,
      TotalAmount: 0,
      isB2B: false,
      B2BId: null,
      PhoneNumber: '',
      Title: '',
      FirstName: '',
      MiddleName: '',
      LastName: '',
      dOB: '',
      Age: 0,
      Gender: '',
      Email: '',
      Address: '',
      CityId: null,
      ServiceId: null,
      IsActive: true,
    });


  }


  setForm() {
    this.sampleRegisterForm = this.fb.group({
      SampleRegisterId: [{ value: 0, disabled: true }],
      Date: ['', Validators.required],
      BranchId: [null, Validators.required],
      TotalAmount: [0, Validators.required],
      isB2B: [false],
      B2BId: [null, Validators.required],
      PhoneNumber: ['', Validators.required],
      Title: ['', Validators.required],
      FirstName: ['', Validators.required],
      MiddleName: ['', Validators.required],
      LastName: ['', Validators.required],
      dOB: ['', Validators.required],
      Age: [{ value: 0, disabled: true }, Validators.required],
      Gender: ['', Validators.required],
      Email: ['', [Validators.required, Validators.email]],
      CityId: [null, Validators.required],
      AreaId: [null, Validators.required],
      Address: ['', Validators.required],
      DoctorId: [null, Validators.required],
      ServiceId: [null, Validators.required],
      PaymentId: [null, Validators.required],
      Amount: [{ value: '', disabled: true }],
      ChequeNo: [{ value: '', disabled: true }],
      ChequeDate: [{ value: '', disabled: true }],
      transactionId: [{ value: '', disabled: true }],
      isActive: [true],
    });
  }


  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      sampleRegisterId: '',
      name: '',
      isActive: true
    };
  }


  loadCities() {
    this.sampleRegisterService.getcities().subscribe({
      next: (res) => {
        this.cities = res || [];
      },
      error: (err) => {
        console.error('Error loading cities:', err);
      }
    });
  }

  loadAreas() {
    const selectedCityId = this.sampleRegisterForm.get('CityId')?.value;
    if (selectedCityId) {
      this.sampleRegisterService.getAreas().subscribe({
        next: (res) => {
          this.areas = (res.data || []).filter(
            (area: { cityId: number }) => area.cityId === Number(selectedCityId)
          );
        },
        error: (err) => {
          console.error('Error loading areas:', err);
          this.areas = [];
        }
      });
    } else {
      this.areas = [];
    }
  }


  loadBranches() {
    this.sampleRegisterService.getBranches().subscribe({
      next: (res) => {
        this.branches = res || [];
      },
      error: (err) => {
        console.error('Error loading branches:', err);
      }
    });
  }

  loadB2Bs() {
    this.sampleRegisterService.getB2Bs().subscribe({
      next: (res) => {
        this.b2bs = res || [];
      },
      error: (err) => {
        console.error('Error loading B2Bs:', err);
      }
    });
  }

  loadServices() {
    this.sampleRegisterService.getServices().subscribe({
      next: (res) => {
        this.services = res || [];
      },
      error: (err) => {
        console.error('Error loading services:', err);
      }
    });
  }

  loadPayments() {
    this.sampleRegisterService.getPayments().subscribe({
      next: (res) => {
        this.payments = res || [];
      },
      error: (err) => {
        console.error('Error loading payments:', err);
      }
    });
  }

  addService() {
    const serviceId = this.sampleRegisterForm.value.ServiceId;
    const selectedService = this.services.find(s => s.serviceId === serviceId);
    if (!selectedService) return;

    const alreadyExists = this.selectedServices.some(s => s.serviceId === serviceId);
    if (!alreadyExists) {
      this.selectedServices.push(selectedService);
      this.calculateTotalAmount();
    } else {
      this.showSuccess('Service already exists in the list');
    }

    this.sampleRegisterForm.patchValue({ ServiceId: null });
  }



  calculateTotalAmount() {
    const isB2B = this.sampleRegisterForm.get('isB2B')?.value;
    const total = this.selectedServices.reduce((sum, service) => {
      const amount = Number(isB2B ? service.b2BAmount : service.b2CAmount) || 0;
      return sum + amount;
    }, 0);

    this.sampleRegisterForm.patchValue({ TotalAmount: total });
  }


  onServiceChange(event: any): void {
    const selectedServices = event.target.value;
  }


  getSampleRegister() {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.name?.trim() || '',
      id: this.searchCriteria.sampleRegisterId || null,
      isActive: this.searchCriteria.isActive
    };

    this.sampleRegisterService.fetchSampleRegister(filter).subscribe({
      next: (res) => {
        this.sampleRegisterList = res.data || [];
      },
      error: (err) => {
        console.error('Error loading services:', err);
      }
    });
  }

  onEdit(sampleRegisterId: number): void {
    this.sampleRegisterService.getSampleRegisterById(sampleRegisterId).subscribe({
      next: (res) => {
        this.sampleRegisterForm.patchValue({
          SampleRegisterId: res.sampleRegisterId,
          Date: res.date ? formatDate(res.date, 'yyyy-MM-dd', 'en-US') : '',
          BranchId: res.branchId,
          TotalAmount: res.totalAmount,
          isB2B: res.isB2B,
          B2BId: res.b2BId,
          PhoneNumber: res.phoneNumber,
          Title: res.title,
          FirstName: res.firstName,
          MiddleName: res.middleName,
          LastName: res.lastName,
          dOB: res.dob ? formatDate(res.dob, 'yyyy-MM-dd', 'en-US') : '',
          Age: res.age,
          Gender: res.gender,
          Email: res.email,
          CityId: res.cityId,
          AreaId: res.areaId,
          Address: res.address,
          DoctorId: res.doctorId,
          PaymentId: res.paymentMapping?.[0]?.paymentId || null,
          Amount: res.amount,
          ChequeNo: res.chequeNo || '',
          ChequeDate: res.chequeDate ? formatDate(res.chequeDate, 'yyyy-MM-dd', 'en-US') : '',
          transactionId: res.transactionId || '',
          isActive: res.isActive,
        });

        // Set services and total
        this.selectedServices = res.serviceMapping || [];
        this.calculateTotalAmount();

        // Handle payment mode UI
        const payment = res.paymentMapping?.[0];
        if (payment) {
          this.selectedPayment = payment.paymentName;
          this.onPaymentModeChange({ target: { value: payment.paymentId } });
        }

        this.isEditModal = true;
        this.openModal();
      },
      error: (err) => {
        console.error('Error loading sample:', err);
      }
    });
  }



  // Toast messages
  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }


  onDelete(sampleRegisterId: number) {
    const sample = this.sampleRegisterList.find(s => s.sampleRegisterId === sampleRegisterId);
    if (sample && sample.isActive === null) {
      sample.isActive = false;
    }

    this.sampleRegisterService.deleteSampleRegister(sampleRegisterId).subscribe(() => {
      const message = sample?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getSampleRegister();
      this.closeModal();
    });
  }

  removeService(service: any): void {
    this.sampleRegisterService.deleteSampleServiceMapId(service.serviceId).subscribe(() => {
      this.selectedServices = this.selectedServices.filter(s => s.serviceId !== service.serviceId);
      this.showSuccess('Service removed successfully');
      this.sampleRegisterForm.patchValue({ ServiceId: null });
      this.calculateTotalAmount();
    });
  }



  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    if (this.sampleRegisterForm.invalid) {
      return;
    }

    if (this.sampleRegisterForm.invalid) {
      this.showError('Please fix validation errors before submitting.');
      return;
    }
    const formValues = this.sampleRegisterForm.getRawValue();

    // console.log('Form value after patch:', this.sampleRegisterForm.getRawValue());

    const selectedPayment = this.payments.find(p => p.paymentId === formValues.PaymentId);

    const payload = {
      SampleRegisterId: formValues.SampleRegisterId,
      Date: formValues.Date,
      BranchId: formValues.BranchId,
      TotalAmount: formValues.TotalAmount,
      isB2B: formValues.isB2B,
      B2BId: formValues.B2BId,
      PhoneNumber: formValues.PhoneNumber,
      Title: formValues.Title,
      FirstName: formValues.FirstName,
      MiddleName: formValues.MiddleName,
      LastName: formValues.LastName,
      dOB: formValues.dOB,
      Age: formValues.Age,
      Gender: formValues.Gender,
      Email: formValues.Email,
      CityId: formValues.CityId,
      AreaId: formValues.AreaId,
      Address: formValues.Address,
      Amount: formValues.Amount,
      ChequeNo: formValues.ChequeNo ?? null,
      ChequeDate: formValues.ChequeDate ?? null,
      transactionId: formValues.transactionId ?? null,
      isActive: formValues.isActive,
      paymentMapping: selectedPayment ? [{
        paymentId: selectedPayment.paymentId,
        paymentName: selectedPayment.paymentName,
        isCash: selectedPayment.isCash,
        isCheque: selectedPayment.isCheque,
        isOnline: selectedPayment.isOnline,
      }] : [],
      serviceMapping: this.selectedServices.length > 0 ? this.selectedServices.map(s => ({
        serviceId: s.serviceId,
        serviceCode: s.serviceCode,
        serviceName: s.serviceName,
        b2BAmount: s.b2BAmount,
        b2CAmount: s.b2CAmount,
        isActive: s.isActive,
      })) : [],
    };

    this.sampleRegisterService.addUpdatedSampleRegister(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
          this.getSampleRegister();
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

  onInputChange(): void {
    Object.keys(this.sampleRegisterForm.controls).forEach((field) => {
      const control = this.sampleRegisterForm.get(field);
      if (control && control.errors) {
        control.setErrors(null);
      }
    });

    this.validationErrors = [];
    this.errorMessage = '';
  }


  calculateAge(dOB: string): void {
    const birthDate = new Date(dOB);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    // Adjust age if birthday hasn't occurred this year yet
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }

    this.sampleRegisterForm.patchValue({ Age: age });
  }


  onPaymentModeChange(event: any): void {
    const selectedPaymentMode = +(event.target as HTMLSelectElement).value;
    const selectedPayment = this.payments.find(p => p.paymentId === selectedPaymentMode);
    this.selectedPayment = selectedPayment?.paymentName || '';
    this.resetField();

    if (!selectedPayment) return;

    const form = this.sampleRegisterForm;

    form.get('PaymentId')?.patchValue(selectedPaymentMode);


    if (!this.isEditModal) {
      form.get('ChequeNo')?.reset();
      form.get('ChequeDate')?.reset();
      form.get('transactionId')?.reset();
    }


    form.get('ChequeNo')?.disable();
    form.get('ChequeDate')?.disable();
    form.get('transactionId')?.disable();


    if (this.selectedPayment === 'Cash') {
      form.get('Amount')?.enable();
    } else if (this.selectedPayment === 'Cheque') {
      form.get('Amount')?.enable();
      form.get('ChequeNo')?.enable();
      form.get('ChequeDate')?.enable();
    } else if (this.selectedPayment === 'Scanner' || this.selectedPayment === 'Online') {
      form.get('Amount')?.enable();
      form.get('transactionId')?.enable();
    }
  }

  resetField(): void {
    this.sampleRegisterForm.get('Amount')?.disable();
    this.sampleRegisterForm.get('ChequeNo')?.disable();
    this.sampleRegisterForm.get('ChequeDate')?.disable();
    this.sampleRegisterForm.get('transactionId')?.disable();

    if (!this.isEditModal) {
      this.sampleRegisterForm.patchValue({
        amount: '',
        chequeNo: '',
        chequeDate: '',
        transactionId: ''
      });
    }
  }



}

