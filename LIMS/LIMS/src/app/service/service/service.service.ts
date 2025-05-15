import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ServiceService {

  constructor(private http: HttpClient) { }

  addUpdatedService(data: any) {
    return this.http.post<any>('https://localhost:7161/api/Service/AddUpdatedService', data);
  }

  fetchService(filter: any) {
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

    return this.http.get<any>('https://localhost:7161/api/Service/GetServiceByFilter', { params });
  }



  deleteService(serviceId: number) {
    return this.http.delete('https://localhost:7161/api/Service/DeleteServiceById?ServiceId=' + serviceId)
  }


  deleteServiceTestId(ServiceTestId: number) {
    return this.http.delete('https://localhost:7161/api/Service/DeleteServiceMapTestById?ServiceTestId='+ ServiceTestId)
  }

  getServiceById(serviceId: number) {
    return this.http.get<any>('https://localhost:7161/api/Service/GetServiceById?ServiceId=' + serviceId);
  }

  GetTests() {  
    return this.http.get<any>('https://localhost:7161/api/Test/GetTestIsActive');
  }

}
