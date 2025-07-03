import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { UserloginService } from '../../../service/AccountService/userLogin/userlogin.service';
import { Router, RouterModule, RouterOutlet } from '@angular/router';
import { userregistration } from '../../../modal/AccountModal/UserRegistrationModal/userregistraionmodal';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
 @ViewChild('autofocus') autofocus !: ElementRef;

  loginFrom: FormGroup = new FormGroup({});
  forgotPasswordForm: FormGroup = new FormGroup({});

  errorMessage: string = '';
  detailsList: userregistration[] = [];
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

    ngAfterViewInit() {
      this.autofocus.nativeElement.focus();
    }

  setFrom2() {
    this.forgotPasswordForm = this.fb.group({
      email: ['', Validators.required],
      otp: ['', Validators.required],
      newPassword: ['', Validators.required],
    });
  }

  setFrom() {
    this.loginFrom = this.fb.group({
      userName: ['', Validators.required],
      password: ['', Validators.required],
      rememberMe: [false],
    });
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
        if (res.success) {
          console.log(res);
          this.showSuccess(res.message);
          this.router.navigate(['/slidebar']);
          this.GetUserLoginDetails();
          const user = this.loginFrom.get('userName')?.value;
          sessionStorage.setItem('loggedInUser', JSON.stringify(user));
        }
        else if (res.errors) {
          this.validationErrors = res.errors;
        }
      },
      error: (err) => {
        if (err.status === 400 && err.error?.errors) {
          this.validationErrors = err.error.errors;
        } else if (err.status === 500 && err.error?.message) {
          this.showError(err.error.message);
        }
        else {
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
        } else if (err.status === 400 && err.error?.message) {
          this.showError(err.error.message);
        } else if (err.status === 500 && err.error?.message) {
          this.showError(err.error.message);
        } else {
          this.errorMessage = 'An unexpected error occurred.';
          this.showError(this.errorMessage);
        }
      }
    })
  }

  VerifyOtp() {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];

    const toEmail = this.forgotPasswordForm.get('email')?.value;
    const enteredOtp = this.forgotPasswordForm.get('otp')?.value;

    const formData = new FormData();
    formData.append('toEmail', toEmail);
    formData.append('enteredOtp', enteredOtp);

    this.userLoginService.VerifyOtp(toEmail, enteredOtp).subscribe({
      next: (res) => {
        console.log(res);
        this.showSuccess(res.message);
        this.showOtpForm = false;
        this.showChangePassword = true;
      },
      error: (err) => {
        if (err.status === 400 && err.error?.errors) {
          this.validationErrors = err.error.errors;
        }else if (err.status === 400 && err.error?.message) {
          this.showError(err.error.message);
          } else if (err.status === 500 && err.error?.message) {
          this.showError(err.error.message);
        } else {
          this.errorMessage = 'An unexpected error occurred';
          this.showError(this.errorMessage);
        }
      }
    })
  }


  updatePassword() {
    this.submitted = true;
    this.errorMessage = '';
    this.validationErrors = [];


    const toEmail = this.forgotPasswordForm.get('email')?.value;
    const newPassword = this.forgotPasswordForm.get('newPassword')?.value;

    const formData = new FormData();
    formData.append('toEmail', toEmail);
    formData.append('newPassword', newPassword);

    this.userLoginService.updateUserLoginPassword(toEmail, newPassword).subscribe({
      next: (res) => {
        console.log(res);
        this.showSuccess(res.message);
        this.showChangePassword = false;
        this.showForgotPassword = false;
        this.router.navigate(['']);
      },
      error: (err) => {
        if (err.status === 400 && err.error?.errors) {
          this.validationErrors = err.error.errors;
        } else if (err.status === 500 && err.error?.message) {
          this.showError(err.error.message);
        } else {
          this.errorMessage = 'An unexpected error occurred';
          this.showError(this.errorMessage);
        }
      }
    });
  }

  GetUserLoginDetails() {
    const UserName = this.loginFrom.get('userName')?.value;
    const Password = this.loginFrom.get('password')?.value;

    const formData = new FormData();
    formData.append('UserName', UserName);
    formData.append('Password', Password);

    this.userLoginService.GetuserLogindetails(UserName, Password).subscribe({
      next: (response) => {
        console.log(response);
        this.detailsList = response;
      },
    })
  }
}
