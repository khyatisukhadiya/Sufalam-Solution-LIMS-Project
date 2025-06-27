import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
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
  constructor(private fb: FormBuilder) { }

  ngOnInit(): void {
    this.setFrom();
  }

  setFrom() {
    this.loginFrom = this.fb.group({
      email: ['', Validators.email],
      password: ['', Validators.required],
      rememberMe: [false]
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

}
