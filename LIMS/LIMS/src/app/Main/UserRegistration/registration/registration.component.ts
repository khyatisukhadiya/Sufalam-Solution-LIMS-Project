import { Component, OnInit, inject } from '@angular/core';
import { UserregistrationService } from '../../../service/AccountService/userregistration/userregistration.service';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { userregistration } from '../../../modal/AccountModal/UserRegistrationModal/userregistraionmodal';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-registration',
  imports: [FormsModule,ReactiveFormsModule,CommonModule],
  templateUrl: './registration.component.html',
  styleUrl: './registration.component.css'
})

export class RegistrationComponent implements OnInit {

  userRegistrationService = inject(UserregistrationService)
  userRegisterForm: FormGroup = new FormGroup({});
  errorMessage: string = '';
  validationErrors: string[] = [];
  userRegisterList : userregistration[] = [];
  submitted : boolean = false;


  constructor(private fb: FormBuilder, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.setFrom();
   }

  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

   onInputChange(): void {
    Object.keys(this.userRegisterForm.controls).forEach((field) => {
      const control = this.userRegisterForm.get(field);
      if (control && control.errors) {
        control.setErrors(null);
      }
    });

    this.validationErrors = [];
    this.errorMessage = '';
  }

  setFrom() {
    this.userRegisterForm = this.fb.group({
      userName: ['', Validators.required],
      fullName: ['', Validators.required],
      email: ['', Validators.required, Validators.email],
      phoneNumber: ['', Validators.required],
      password: ['', Validators.required, Validators.minLength(6)],
      confirmPassword: ['', Validators.required, Validators.minLength(6)],
      gender: [null],
      dOB: [null],
    });
  }

  onSubmit() {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

     if(this.userRegisterForm.invalid){
       console.log("from", this.userRegisterForm.value);
      return;
     }

    const payload = this.userRegisterForm.getRawValue();

    this.userRegistrationService.AddUserRegistartion(payload).subscribe({
      next: (res) => {
        console.log(res);
        this.toastr.success(res.message);
        if (res.success) {
          this.showSuccess(res.message);
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


}
