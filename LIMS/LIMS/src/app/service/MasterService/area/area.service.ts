import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AreaService {

  constructor(private http: HttpClient) {}

  // Add or update an area
  addUpdateArea(data: any) {
    return this.http.post<any>('https://localhost:7161/api/Area/AddUpdatedArea', data);
  }

  // Get all active Cities (previously GetStates)
  getCities() {
    return this.http.get<any>('https://localhost:7161/api/City/GetCityIsActive');
  }

  // Fetch filtered areas
  fetchAreas(filter: any) {
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

    return this.http.get<any>('https://localhost:7161/api/Area/GetAllArea', { params });
  }

  // Get a single area by ID
  getAreaById(areaId: number) {
    return this.http.get<any>('https://localhost:7161/api/Area/GetAreaById?AreaId=' + areaId);
  }

  // Toggle active status of an area
  deleteArea(areaId: number) {
    return this.http.delete('https://localhost:7161/api/Area/DeleteAreaById?AreaId=' + areaId);
  }
}
