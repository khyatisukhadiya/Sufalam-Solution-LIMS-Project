import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UserloginService {

  constructor(private http : HttpClient) { }

  UserLogin(data : any){
    return this.http.post<any>('https://localhost:7161/api/UserLogin/UserLogin', data);
  }

  sendOtp(toEmail : string, subject: string, body: string, otp : string){
    const formData = new FormData();
    formData.append('toEmail', toEmail);
    formData.append('subject', subject);
    formData.append('body', body);
    formData.append('otp', otp);
    return this.http.post<any>('https://localhost:7161/api/Email/SendOtpToEmail?toEmail=', formData);
  }


  VerifyOtp(enteredOtp : string){
    return this.http.post<any>('https://localhost:7161/api/Email/VerifyOtp?enteredOtp=', enteredOtp)
  }
}
