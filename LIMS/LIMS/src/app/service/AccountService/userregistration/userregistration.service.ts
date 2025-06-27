import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UserregistrationService {

  constructor(private http : HttpClient) { }

  AddUserRegistartion(data : any) {
    return this.http.post('https://localhost:7161/api/UserRegistration/AddUserRegistration', data);
  }
}
