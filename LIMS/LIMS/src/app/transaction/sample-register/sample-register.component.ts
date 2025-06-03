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
import { doctorModal } from '../../modal/MasterModel/doctorModal';
import { serviceModal } from '../../modal/MasterModel/serviceModal';
import { paymentModel } from '../../modal/FinanceModel/payment';

@Component({
  selector: 'app-sample-register',
  imports: [SliderbarComponent, RouterModule, CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './sample-register.component.html',
  styleUrl: './sample-register.component.css'
})
export class SampleRegisterComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
  @ViewChild('autofocus') autofocus!: ElementRef;

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
  services: serviceModal[] = [];
  payments: paymentModel[] = [];
  doctors: doctorModal[] = [];
  selectedServices: any[] = [];
  selectedPayment: string = '';
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';
  searchCriteria = { sampleRegisterId: '', name: '', date: '' };
  changeDetectorRef: any;


  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  sampleRegisterService = inject(SampleRegisterService);

  ngOnInit(): void {
    this.setForm();


    this.sampleRegisterForm.get('dob')?.valueChanges.subscribe((dobValue: string) => {
      if (dobValue) {
        this.calculateAge(dobValue);
      } else {
        this.sampleRegisterForm.patchValue({ age: 0 });
      }
    });

    this.sampleRegisterForm.get('isB2B')?.valueChanges.subscribe((value: boolean) => {
      this.isB2B = value;
      this.calculateTotalAmount();
    });



    this.sampleRegisterForm.get('cityId')?.valueChanges.subscribe(() => {
      this.loadAreas();
    });


    this.loadCities();
    // this.loadAreas();
    this.loadBranches();
    this.loadB2Bs();
    this.loadServices();
    this.loadPayments();
    this.loadDoctors();
  }

  openModal() {
    this.submitted = false;
    this.validationErrors = [];
    const modalEl = document.getElementById('myModal');
    if (modalEl != null) {
      modalEl.style.display = "block";
      modalEl.addEventListener('shown.bs.modal', () => {
        if (this.autofocus) {
          this.autofocus.nativeElement.focus();
        }
      });
    }

    if (!this.isEditModal) {
      const today = new Date().toISOString().split('T')[0];
      this.sampleRegisterForm.patchValue({ date: today });
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
      sampleRegisterId: 0,
      date: '',
      branchId: null,
      areaId: null,
      totalAmount: 0,
      isB2B: false,
      b2BId: null,
      phoneNumber: '',
      title: '',
      firstName: '',
      middleName: '',
      lastName: '',
      dob: '',
      age: 0,
      gender: '',
      email: '',
      address: '',
      cityId: null,
      doctorId: null,
      isActive: true,
      paymentId: '',
      // paymentMapping: [],
      selectedPayment: [],
      amount: '',
      chequeNo: '',
      chequeDate: '',
      transactionId: ''
    });


  }


  setForm() {
    this.sampleRegisterForm = this.fb.group({
      sampleRegisterId: [{ value: 0, disabled: true }],
      date: [{ value: '', disabled: true }, Validators.required],
      branchId: [null],
      totalAmount: [0, Validators.required],
      isB2B: [false],
      b2BId: [null],
      phoneNumber: ['', Validators.required],
      title: ['', Validators.required],
      firstName: ['', Validators.required],
      middleName: ['', Validators.required],
      lastName: ['', Validators.required],
      dob: ['', Validators.required],
      age: [{ value: 0, disabled: true }, Validators.required],
      gender: ['', Validators.required],
      email: ['', [Validators.email]],
      cityId: [null],
      areaId: [null],
      address: [''],
      doctorId: [null],
      serviceId: [null, Validators.required],
      paymentId: [null, Validators.required],
      Amount: [{ value: '', disabled: true }, Validators.required],
      ChequeNo: [{ value: '', disabled: true }, Validators.required],
      ChequeDate: [{ value: '', disabled: true }, Validators.required],
      TransactionId: [{ value: '', disabled: true }, Validators.required],
      regBy: ['', Validators.required],
    });
  }


  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      sampleRegisterId: '',
      name: '',
      date: ''
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
    const selectedCityId = this.sampleRegisterForm.get('cityId')?.value;
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
        console.log('Payments response:', res);
        this.payments = res || [];
      },
      error: (err) => {
        console.error('Error loading payments:', err);
      }
    });
  }

  loadDoctors() {
    this.sampleRegisterService.getDoctors().subscribe({
      next: (res) => {
        this.doctors = res || [];
      },
      error: (err) => {
        console.error('Error loading doctors:', err);
      }
    });
  }

  addService() {
    const serviceId = this.sampleRegisterForm.value.serviceId;
    const selectedService = this.services.find(s => s.serviceId === serviceId);
    if (!selectedService) return;

    const alreadyExists = this.selectedServices.some(s => s.serviceId === serviceId);
    if (!alreadyExists) {
      this.selectedServices.push(selectedService);
      this.calculateTotalAmount();
    } else {
      this.showSuccess('Service already exists in the list');
    }

    this.sampleRegisterForm.patchValue({ serviceId: null });
  }



  calculateTotalAmount() {
    const isB2B = this.sampleRegisterForm.get('isB2B')?.value;
    const total = this.selectedServices.reduce((sum, service) => {
      const amount = Number(isB2B ? service.b2BAmount : service.b2CAmount) || 0;
      return sum + amount;
    }, 0);

    this.sampleRegisterForm.patchValue({ totalAmount: total });
  }


  // onServiceChange(event: any): void {
  //   const selectedServices = event.target.value;
  // }

  // onBranchChnage(event : any) : void {
  //   const selectedBranch = event.target.value;
  // }

  getSampleRegister() {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.name?.trim() || '',
      id: this.searchCriteria.sampleRegisterId || null,
      date: this.searchCriteria.date
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

        // console.log('Sample Register Data before :', this.sampleRegisterForm);
        this.sampleRegisterForm.patchValue(
          {
            sampleRegisterId: res.sampleRegisterId,
            date: res.date ? formatDate(res.date, 'yyyy-MM-dd', 'en-US') : '',
            branchId: res.branchId,
            totalAmount: res.totalAmount,
            isB2B: res.isB2B,
            b2BId: res.b2BId ? res.b2BId : null,
            phoneNumber: res.phoneNumber,
            title: res.title,
            firstName: res.firstName,
            middleName: res.middleName,
            lastName: res.lastName,
            dob: res.dob ? formatDate(res.dob, 'yyyy-MM-dd', 'en-US') : '',
            age: res.age,
            gender: res.gender,
            email: res.email ? res.email : null,
            cityId: res.cityId,
            areaId: res.areaId,
            address: res.address,
            isActive: res.isActive,
            Amount: res.amount,
            ChequeNo: res.chequeNo,
            ChequeDate: res.chequeDate ? formatDate(res.chequeDate, 'yyyy-MM-dd', 'en-US') : null,
            TransactionId: res.transactionId,
            // paymentId: res.paymentMapping?.[0]?.paymentId,
            regBy: res.createdBy,
            doctorId: res.doctorId,
            paymentId: res.paymentId,
          }

        );
        // console.log('Sample Register Data:', this.sampleRegisterForm);
        // this.sampleRegisterForm.get('amount')?.enable();
        this.sampleRegisterForm.get('chequeNo')?.enable();
        this.sampleRegisterForm.get('chequeDate')?.enable();
        this.sampleRegisterForm.get('transactionId')?.enable();

        console.log('transactionId:', res.transactionId);
        console.log('chequeDate:', res.chequeDate);
        console.log('chequeNo:', res.chequeNo);
        console.log('amount:', res.amount);


        this.selectedServices = res.serviceMapping || [];
        this.calculateTotalAmount();


        if (res.paymentId) {
          const payment = this.payments.find(p => p.paymentId === res.paymentId);
          if (payment) {
            this.selectedPayment = payment.paymentName;
            // Directly pass the value, not an event
            this.onPaymentModeChange({ target: { value: payment.paymentId.toString() } });
          }
        }


        this.isEditModal = true;
        this.openModal();
        this.getSampleRegister();

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
    this.sampleRegisterService.deleteSampleServiceMapId(service.sampleServiceMapId).subscribe(() => {
      this.selectedServices = this.selectedServices.filter(s => s.sampleServiceMapId !== service.sampleServiceMapId);
      this.showSuccess('Service removed successfully');
      this.sampleRegisterForm.patchValue({ sampleServiceMapId: null });
      this.calculateTotalAmount();
    });
  }



  onSubmit(): void {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];


    if (this.sampleRegisterForm.invalid) {
      // this.changeDetectorRef.detectChanges()
      // this.sampleRegisterForm.markAllAsTouched();
      // this.showError('Please fix the validation errors before submitting.');
      return;
    }

    // if (this.sampleRegisterForm.valid) {
    //   const formValues = this.sampleRegisterForm.value;
    //   this.sampleRegisterService.addUpdatedSampleRegister(formValues).subscribe((response) => {
    //     console.log("Data added/updated successfully:", response);
    //   },
    //     (error) => {
    //       console.error("Error adding/updating data:", error);
    //     }
    //   );
    // } else {
    //   // this.markFormGroupTouched(this.sampleRegisterForm);
    // }


    const formValues = this.sampleRegisterForm.getRawValue();


    console.log('Form Values:', formValues);


    const selectedPayment = this.payments.find(p =>
      p.paymentId === +formValues.paymentId
    );

    if (!selectedPayment) {
      this.showError('Invalid or missing payment selection.');
      return;
    }

    if (!this.selectedServices || this.selectedServices.length === 0) {
      this.showError('Please select at least one service.');
      return;
    }

    console.log("sampleregister", this.sampleRegisterForm);

    const payload = {
      sampleRegisterId: formValues.sampleRegisterId,
      date: formValues.date,
      branchId: formValues.branchId,
      branchName: this.branches.find(b => b.branchId === formValues.branchId)?.branchName || '',
      totalAmount: formValues.totalAmount,
      isB2B: formValues.isB2B,
      b2BId: formValues.b2BId ?? null,
      b2BName: this.b2bs.find(b => b.b2BId === formValues.b2BId)?.b2BName || '',
      phoneNumber: formValues.phoneNumber,
      title: formValues.title,
      firstName: formValues.firstName,
      middleName: formValues.middleName,
      lastName: formValues.lastName,
      dob: formValues.dob,
      age: formValues.age,
      gender: formValues.gender,
      email: formValues.email ?? null,
      cityId: formValues.cityId,
      cityName: this.cities.find(c => c.cityId === formValues.cityId)?.cityName || '',
      areaId: formValues.areaId,
      areaName: this.areas.find(a => a.areaId === formValues.areaId)?.areaName || '',
      address: formValues.address,
      amount: formValues.Amount,
      chequeNo: formValues.ChequeNo || null,
      chequeDate: formValues.ChequeDate || null,
      transactionId: formValues.TransactionId || null,
      regBy: formValues.regBy || null,
      doctorId: formValues.doctorId,
      doctorName: this.doctors.find(d => d.doctorId === formValues.doctorId)?.doctorName || '',
      paymentId: selectedPayment.paymentId,
      paymentName: selectedPayment.paymentName,
      // isActive: true,
      // paymentMapping: [{
      //   paymentId: selectedPayment.paymentId,
      //   paymentName: selectedPayment.paymentName,
      //   isCash: selectedPayment.isCash,
      //   isCheque: selectedPayment.isCheque,
      //   isOnline: selectedPayment.isOnline,
      //   isActive: true
      // }],
      serviceMapping: this.selectedServices.map(s => ({
        serviceId: s.serviceId,
        serviceCode: s.serviceCode,
        serviceName: s.serviceName,
        b2BAmount: s.b2BAmount,
        b2CAmount: s.b2CAmount,
        isActive: true,
        sampleServiceMapId: s.sampleServiceMapId ?? 0
      }))
    };

    console.log("error", this.sampleRegisterForm.valid);
    console.log("before sampleregister", this.sampleRegisterForm)
    console.log('Final Payload:', payload);

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


  calculateAge(dob: string): void {
    const birthDate = new Date(dob);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();


    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }

    this.sampleRegisterForm.patchValue({ age: age });
  }

  onServiceChange(event: any): void {
    const selectedServices = event.target.value;
    this.sampleRegisterForm.patchValue({ services: selectedServices });
    this.onInputChange();
  }

  onBranchChange(event: any): void {
    const selectedBranch = event.target.value;
    this.sampleRegisterForm.patchValue({ branchId: selectedBranch });
    this.onInputChange();
  }

  onCityChange(event: any): void {
    const selectedCity = event.target.value;
    this.sampleRegisterForm.patchValue({ cityId: selectedCity });
    this.onInputChange();
    this.loadAreas();
  }

  onAreaChange(event: any): void {
    const selectedArea = event.target.value;
    this.sampleRegisterForm.patchValue({ areaId: selectedArea });
    this.onInputChange();
  }

  onDoctorChange(event: any): void {
    const selectedDoctor = event.target.value;
    this.sampleRegisterForm.patchValue({ doctorId: selectedDoctor });
    this.onInputChange();
  }

  onPaymentModeChange(event: any): void {
    const selectedPaymentMode = +(event.target as HTMLSelectElement).value;
    const selectedPayment = this.payments.find(p => p.paymentId === selectedPaymentMode);
    this.selectedPayment = selectedPayment?.paymentName || '';
    this.resetField();

    if (!selectedPayment) return;

    const form = this.sampleRegisterForm;

    form.get('paymentId')?.patchValue(selectedPaymentMode);


    if (!this.isEditModal) {
      // form.get('amount')?.reset();
      form.get('chequeNo')?.reset();
      form.get('chequeDate')?.reset();
      form.get('transactionId')?.reset();
    }


    // form.get('chequeNo')?.disable();
    // form.get('chequeDate')?.disable();
    // form.get('transactionId')?.disable();


    if (this.selectedPayment === 'Cash') {
      form.get('Amount')?.enable();
    } else if (this.selectedPayment === 'Cheque') {
      form.get('Amount')?.enable();
      form.get('ChequeNo')?.enable();
      form.get('ChequeDate')?.enable();
    } else if (this.selectedPayment === 'Scanner' || this.selectedPayment === 'Online') {
      form.get('Amount')?.enable();
      form.get('TransactionId')?.enable();
    }
  }

  resetField(): void {
    // this.sampleRegisterForm.get('Amount')?.disable();
    // this.sampleRegisterForm.get('ChequeNo')?.disable();
    // this.sampleRegisterForm.get('ChequeDate')?.disable();
    // this.sampleRegisterForm.get('TransactionId')?.disable();

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