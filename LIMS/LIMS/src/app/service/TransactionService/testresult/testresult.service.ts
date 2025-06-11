import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TestresultService {

  constructor(private http : HttpClient) { }

  getSampleregister(filter : any){
    let params = new HttpParams();

    if (filter.name) {
      params = params.set('name', filter.name);
    }

    if (filter.phoneNumber) {
      params = params.set('code', filter.phoneNumber);
    }

    if (filter.id !== null && filter.id !== undefined) {
      params = params.set('id', filter.id.toString());
    }

    if (filter.isActive !== null && filter.isActive !== undefined) {
      params = params.set('isActive', filter.isActive.toString());
    }
    return this.http.get<any>('https://localhost:7161/api/SampleRegister/GetSampleByFilter', {params});
  }

  getServices() {
    return this.http.get<any>('https://localhost:7161/api/Service/GetServiceIsActive');
  }

  getSampleRegisterById(sampleRegisterId : number) {
    return this.http.get<any>(`https://localhost:7161/api/SampleRegister/GetSampleRegisterById?SampleRegisterId=${sampleRegisterId}`);
  }
}
