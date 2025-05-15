import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})

export class StateService {  

  constructor(private http: HttpClient) { }


  addUpdatedState(data: any) {  
    return this.http.post<any>('https://localhost:7161/api/State/AddUpdatedState', data);  
  }


  GetCountry() {
    return this.http.get<any>('https://localhost:7161/api/Country/GetCountryIsActive');
  }


  fetchState(filter: any) { 
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

    if (filter.isActive !== null && filter.isActive !== undefined)   {
      params = params.set('isActive', filter.isActive.toString());
    }

    return this.http.get<any>('https://localhost:7161/api/State/GetAllStates', { params });  

  }

  GetStateById(StateId : number){
    return this.http.get<any>('https://localhost:7161/api/State/GetStateById?StateId='+StateId)
  }

  deleteState(stateId: number) {  
    return this.http.delete('https://localhost:7161/api/State/DeleteStateById?StateId=' + stateId); 
  }
}
