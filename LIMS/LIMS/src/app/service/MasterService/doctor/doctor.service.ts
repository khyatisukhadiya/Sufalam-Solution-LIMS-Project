import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DoctorService {

  constructor(private http: HttpClient) { }

  addUpdatedDoctor(data: any) {
    return this.http.post<any>('https://localhost:7161/api/Doctor/AddUpdatedDoctor', data);
  }

  fetchDoctor(filter: any) {
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

    return this.http.get<any>('https://localhost:7161/api/Doctor/GetDoctorsByFilter', { params });
  }

  deleteDoctor(doctorId: number) {
    return this.http.delete('https://localhost:7161/api/Doctor/DeleteDoctorById?DoctorId=' + doctorId);
  }

  getDoctorById(doctorId: number) {
    return this.http.get<any>('https://localhost:7161/api/Doctor/GetDoctorById?DoctorId=' + doctorId);
  }
}
