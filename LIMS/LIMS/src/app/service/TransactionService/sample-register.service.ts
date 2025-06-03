import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})

export class SampleRegisterService {

  constructor(private http: HttpClient) { }

  getcities() {
    return this.http.get<any>('https://localhost:7161/api/City/GetCityIsActive');
  }


  getAreas() {
    return this.http.get<any>('https://localhost:7161/api/Area/GetAreaIsActive');
  }


  getBranches() {
    return this.http.get<any>('https://localhost:7161/api/Branch/GetBranchIsActive');
  }

  getB2Bs() {
    return this.http.get<any>('https://localhost:7161/api/B2B/GetB2BIsActive');
  }

  getServices() {
    return this.http.get<any>('https://localhost:7161/api/Service/GetServiceIsActive');
  }

  getPayments() {
    return this.http.get<any>('https://localhost:7161/api/Payment/GetPaymentIsActive');
  }

  getDoctors(){
    return this.http.get<any>('https://localhost:7161/api/Doctor/GetDoctorIsActive');
  }

  fetchSampleRegister(filter: any) {
    let params = new HttpParams();

    if (filter.name) {
      params = params.set('name', filter.name);
    }

    if (filter.code) {
      params = params.set('code', filter.code);
    }

    if (filter.id !== null && filter.id !== undefined) {
      params = params.set('id', filter.id.toString());
    }

    if (filter.isActive !== null && filter.isActive !== undefined) {
      params = params.set('isActive', filter.isActive.toString());
    }

    return this.http.get<any>('https://localhost:7161/api/SampleRegister/GetSampleByIsActive', { params });
  }


  addUpdatedSampleRegister(data: any) {
    return this.http.post<any>('https://localhost:7161/api/SampleRegister/AddUpdateSampleRegister', data);
  }

  getSampleRegisterById(sampleRegisterId: number) {
    return this.http.get<any>('https://localhost:7161/api/SampleRegister/GetSampleRegisterById?SampleRegisterId=' + sampleRegisterId);
  }

  deleteSampleRegister(sampleRegisterId: number) {
    return this.http.delete<any>('https://localhost:7161/api/SampleRegister/DeleteSampleRegisterById?SampleRegisterId=' + sampleRegisterId);
  }

  deleteSampleServiceMapId(SampleServiceMapId: number) {
    return this.http.delete<any>('https://localhost:7161/api/SampleRegister/DeleteSampleServiceMapId?SampleServiceMapId=' + SampleServiceMapId);      
  }




}
