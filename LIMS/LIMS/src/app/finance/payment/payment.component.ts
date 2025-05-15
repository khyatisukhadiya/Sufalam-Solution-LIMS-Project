import { CommonModule } from '@angular/common';
import { Component, ElementRef, Inject, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { filter } from 'rxjs';
import { Modal } from 'bootstrap';
import { SliderbarComponent } from "../../component/sliderbar/sliderbar.component";
import { PaymentService } from '../../service/FinanceService/payment/payment.service';
import { paymentModel } from '../../modal/FinanceModel/payment';

@Component({
  selector: 'app-payment',
  imports: [RouterModule, ReactiveFormsModule, FormsModule, CommonModule, SliderbarComponent],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css'
})
export class PaymentComponent implements OnInit {
  @ViewChild('myModal') modal: ElementRef | undefined;
  paymentForm: FormGroup = new FormGroup({});
  paymentService = inject(PaymentService)
  filteredPayments: any = [];
  modalInstance: Modal | undefined;
  payments: any[] = []
  searchClick: boolean = false;
  submitted: boolean = false;
  paymentList: paymentModel[] = [];
  isEditModal = false;
  searchCriteria = { paymentId: '', paymentName: '', isActive: true };
  errorMessage: string = '';
  validationErrors: string[] = [];
  formErrors: any = {};
  toastMessage: string = '';

  constructor(private fb: FormBuilder, private toastr: ToastrService) { }


  ngOnInit(): void {
    this.setForm();
  }


  openModal() {
    const paymentModal = document.getElementById('myModal');

    if (paymentModal != null) {
      paymentModal.style.display = "block";
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

    this.paymentForm.reset({
      PaymentId: 0,
      PaymentName: '',
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
    Object.keys(this.paymentForm.controls).forEach((field) => {
      const control = this.paymentForm.get(field);
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

    if (this.paymentForm.invalid) {
      return;
    }

    const payload = this.paymentForm.getRawValue();

    console.log(payload);

    const isUpdate = payload.PaymentId && payload.PaymentId > 0;

    this.paymentService.addUpdatePayment(payload).subscribe({
      next: (res:any) => {
        if (res.success) {
          this.showSuccess(res.message);
          this.closeModal();
          this.searchClick = true;
          this.getPayment();
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
    this.paymentForm = this.fb.group({
      PaymentId: [{ value: 0, disabled: true }],
      PaymentName: ['', Validators.required],
      PaymentCode: ['', Validators.required],
      isActive: [true],
    });
  }

  getPayment(): void {
    this.searchClick = true;

    const filter = {
      name: this.searchCriteria.paymentName?.trim() || '',
      id: this.searchCriteria.paymentId || null,
      isActive: this.searchCriteria.isActive
    };

    this.paymentService.fetchPayment(filter)
      .subscribe({
        next: (res) => {
          this.paymentList = res?.data || [];
        },
        error: (err) => {
          console.error('Failed to load payments:', err);
        }
      });
  }

  onEdit(paymentId : number) {
    this.paymentService.getPaymentId(paymentId).subscribe({
      next : (res : any) => {
        console.log('onedit', res);

        this.paymentForm.patchValue({
          PaymentId : res.paymentId,
          PaymentName : res.paymentName,
          isActive : res.isActive
        });
        this.isEditModal = true;
        this.openModal();
        this.getPayment();
      }
    }); 
  }

  onDelete(paymentId: number) {
    const payment = this.paymentList.find(c => c.paymentId === paymentId);
    if (payment && payment.isActive === null) {
      payment.isActive = false;
    }

    this.paymentService.deletePaymentId(paymentId).subscribe(() => {
      const message = payment?.isActive ? 'Deactivated' : 'Activated';
      this.showSuccess(message);
      this.getPayment();
      this.closeModal();
    })
  }

  clearSearch() {
    this.searchClick = false;
    this.searchCriteria = {
      paymentId: '',
      paymentName: '',
      isActive: true
    };
  }

}