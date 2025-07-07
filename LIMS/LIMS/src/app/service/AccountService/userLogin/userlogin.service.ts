import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

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


  VerifyOtp(toEmail : string, enteredOtp : string){
    const formData = new FormData();
    formData.append('toEmail', toEmail);
    formData.append('enteredOtp', enteredOtp);
    return this.http.post<any>('https://localhost:7161/api/Email/VerifyOtp?enteredOtp=', formData)
  }


  updateUserLoginPassword(toEmail : string, newPassword : string,){
    const formData = new FormData();
    formData.append('toEmail', toEmail);
    formData.append('newPassword', newPassword);
     return this.http.post<any>('https://localhost:7161/api/UserLogin/ChangeUserPassword?newPassword=', formData)
  }

  GetuserLogindetails(UserName : string, Password : string): Observable<any>{
    const formData = new FormData();
    formData.append('UserName', UserName);
    formData.append('Password',Password);
    return this.http.post<any>('https://localhost:7161/api/UserLogin/GetUserLogindetails',formData)
  }
}
