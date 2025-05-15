import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CountryService {

  constructor(private http : HttpClient) { }
  
  addUpdatedCountry(data : any){
      return this.http.post<any>('https://localhost:7161/api/Country/AddUpdatedCountry',data);
  }
  
  fetchCountry(filter: any) {
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
  
    return this.http.get<any>('https://localhost:7161/api/Country/GetCountriesByFilter', { params });
  }
  
  

  deleteCountry(countryId : number){
    return this.http.delete('https://localhost:7161/api/Country/DeleteCountryById?countryId='+countryId)
  }

  getCountryById(countryId : number){
    return this.http.get<any>('https://localhost:7161/api/Country/GetCountryById?CountryId='+ countryId);
  }
}
