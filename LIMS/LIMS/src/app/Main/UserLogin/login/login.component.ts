import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { UserloginService } from '../../../service/AccountService/userLogin/userlogin.service';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  loginFrom: FormGroup = new FormGroup({});

  errorMessage: string = '';

  // Forgot password states
  showForgotPassword: boolean = false;
  showOtpForm: boolean = false;
  showChangePassword: boolean = false;
  resetEmail: string = '';
  otp: string = '';
  generatedOtp: string = '';
  newPassword: string = '';
  submitted: boolean = false;
  validationErrors: string[] = [];

  constructor(private fb: FormBuilder, private toastr: ToastrService, private router: Router) { }
  userLoginService = inject(UserloginService)

  ngOnInit(): void {
    this.setFrom();
  }

  setFrom() {
    this.loginFrom = this.fb.group({
      email: ['', Validators.email],
      password: ['', Validators.required],
      rememberMe: [false],
      otp : [''],
    })
  }

  toggleForgotPassword(event?: Event): void {
    if (event) {
      event.preventDefault();
    }
    this.showForgotPassword = true;
    this.showOtpForm = false;
    this.showChangePassword = false;
    this.resetEmail = '';
  }

  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }


  onSubmit() {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    if (this.loginFrom.invalid) {
      console.log("loginFrom", this.loginFrom.value);
      return;
    }

    const payload = this.loginFrom.getRawValue();

    this.userLoginService.UserLogin(payload).subscribe({
      next: (res) => {
        console.log(res);
        this.showSuccess(res.message);
        this.router.navigate(['']);
        // if (res.success) {
        //   this.showSuccess(res.message);
        // } else if (res.errors) {
        //   this.validationErrors = res.errors;
        // }
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

  sentOtp() {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    const toEmail = this.loginFrom.get('email')?.value;
    const subject = "Your OTP Code";
    const body = `Your OTP code is ${this.otp}`;
    const otp = this.otp;

    const formData = new FormData();
    formData.append('toEmail', toEmail);
    formData.append('subject', subject);
    formData.append('body', body);
    formData.append('otp', otp);

    this.userLoginService.sendOtp(toEmail, subject, body, otp).subscribe({
      next: (res) => {
        console.log(res);
        this.showSuccess(res.message);
        this.showOtpForm = true;
      },
      error: (err) => {
        if (err.status === 400 && err.error?.errors) {
          this.validationErrors = err.error.errors;
        } else {
          this.errorMessage = 'An unexpected error occurred.';
          this.showError(this.errorMessage);
        }
      }
    })
  }

  VerifyOtp(){
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    const enteredOtp = this.loginFrom.get('otp')?.value;

    this.userLoginService.VerifyOtp(enteredOtp).subscribe({
      next: (res) =>
        {
          console.log(res);
          this.showSuccess(res.message);
        },
        error: (err) => {
        if (err.status === 400 && err.error?.errors) {
          this.validationErrors = err.error.errors;
        } else {
          this.errorMessage = 'An unexpected error occurred';
          this.showError(this.errorMessage);
        }
      }
    })
  }

}
