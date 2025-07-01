import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { UserloginService } from '../../../service/AccountService/userLogin/userlogin.service';
import { Router, RouterModule, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  loginFrom: FormGroup = new FormGroup({});
  forgotPasswordForm : FormGroup = new FormGroup({});

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
    this.setFrom2();
  }

  setFrom2(){
    this.forgotPasswordForm = this.fb.group({
      email: ['', Validators.required],
      otp: ['', Validators.required],
      newPassword: ['', Validators.required],
      });
  }

  setFrom() {
    this.loginFrom = this.fb.group({
       // usernameOrEmail: ['', [Validators.required, this.usernameOrEmailValidator()]],
      email: ['', Validators.email],
      userName : ['', Validators.required],
      password: ['', Validators.required],
      rememberMe: [false],
    });
  }

  //    usernameOrEmailValidator(control: AbstractControl): ValidationErrors | null {
  //    const value = control.value;
  //    if (!value) {
  //      return null; // Or return { required: true } if required
  //    }

  //    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  //    const usernameRegex = /^[a-zA-Z0-9_-]{3,16}$/; // Example: 3-16 characters, letters, numbers, underscore, hyphen

  //    if (emailRegex.test(value) || usernameRegex.test(value)) {
  //      return null;
  //    } else {
  //      return { 'usernameOrEmail': true };
  //    }
  //  }

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
        this.router.navigate(['/slidebar']);

        const user = this.loginFrom.get('userName')?.value;
        sessionStorage.setItem('loggedInUser', JSON.stringify(user));

        // if (res.success) {
        //   this.showSuccess(res.message);
        // } else if (res.errors) {
        //   this.validationErrors = res.errors;
        // }
        // this.showError(res.message);
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

    const toEmail = this.forgotPasswordForm.get('email')?.value;
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

    const toEmail = this.forgotPasswordForm.get('email')?.value;
    const enteredOtp = this.forgotPasswordForm.get('otp')?.value;

    const formData = new FormData();
    formData.append('toEmail', toEmail);
    formData.append('enteredOtp', enteredOtp);

    this.userLoginService.VerifyOtp(toEmail,enteredOtp).subscribe({
      next: (res) =>
        {
          console.log(res);
          this.showSuccess(res.message);
          this.showOtpForm = false;
          this.showChangePassword = true;
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


  updatePassword(){
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];


    const toEmail = this.forgotPasswordForm.get('email')?.value;
    const newPassword = this.forgotPasswordForm.get('newPassword')?.value;

    const formData = new FormData();
    formData.append('toEmail', toEmail);
    formData.append('newPassword', newPassword);

    this.userLoginService.updateUserLoginPassword(toEmail, newPassword).subscribe({
      next: (res) =>
        { 
          console.log(res);
          this.showSuccess(res.message);
          this.showChangePassword = false;
          this.showForgotPassword = false;
          this.router.navigate(['']);
        },
        error: (err) => {
        if (err.status === 400 && err.error?.errors) {
          this.validationErrors = err.error.errors;
        } else {
          this.errorMessage = 'An unexpected error occurred';
          this.showError(this.errorMessage);
        }
  }});
  }

}
