import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CityService {

  constructor(private http: HttpClient) {}

  addUpdatedCity(data: any) {
    return this.http.post<any>('https://localhost:7161/api/City/AddUpdatedCity', data);
  }

  // Get all active States 
  GetStates() {
    return this.http.get<any>('https://localhost:7161/api/State/GetStateIsActivate');
  }

  // Fetch filtered cities
  fetchCity(filter: any) {
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

    return this.http.get<any>('https://localhost:7161/api/City/GetAllCities', { params });
  }

  // Get a single city by ID
  GetCityById(cityId: number) {
    return this.http.get<any>('https://localhost:7161/api/City/GetCityById?CityId=' + cityId);
  }

  // Toggle active status of a city
  deleteCity(cityId: number) {
    return this.http.delete('https://localhost:7161/api/City/DeleteCityById?CityId=' + cityId);
  }
}
