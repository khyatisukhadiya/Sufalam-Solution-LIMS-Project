import { Routes } from '@angular/router';
import { CountryComponent } from './master/country/country.component';
import { StateComponent } from './master/state/state.component';
import { CityComponent } from './master/city/city.component';
import { SliderbarComponent } from './component/sliderbar/sliderbar.component';
import { AreaComponent } from './master/area/area.component';
import { BranchComponent } from './master/branch/branch.component';
import { B2BComponent } from './master/b2b/b2b.component';
import { DoctorComponent } from './master/doctor/doctor.component';
import { TestComponent } from './master/test/test.component';
import { ServiceComponent } from './master/service/service.component';
import { PaymentComponent } from './finance/payment/payment.component';
import { SampleRegisterComponent } from './transaction/sample-register/sample-register.component';
import { TestresultComponent } from './transaction/testresult/testresult.component';
import { TestapprovalComponent } from './transaction/testapproval/testapproval.component';
import { UserregistrationService } from './service/AccountService/userregistration/userregistration.service';
import { RegistrationComponent } from './Main/UserRegistration/registration/registration.component';
import { LoginComponent } from './Main/UserLogin/login/login.component';

export const routes: Routes = [
    {
        path : '',
        component : LoginComponent
    },
    {
        path : 'slidebar',
        component : SliderbarComponent
    },
    {
        path : 'country',
        component : CountryComponent
    },
    {
        path : 'state',
        component : StateComponent
    },
    {
        path : 'city',
        component : CityComponent
    },
    {
        path : 'area',
        component : AreaComponent
    },
    {
        path : 'branch',
        component : BranchComponent
    },
    {
        path : 'b2b',
        component : B2BComponent
    },
    {
        path  : 'doctor',
        component : DoctorComponent
    },
    {
        path : 'test',
        component : TestComponent
    },
    {
        path : 'service',
        component : ServiceComponent
    },
    {
        path : 'payment',
        component : PaymentComponent
    },
    {
        path : 'sampleRegister',
        component : SampleRegisterComponent
    },
    {
        path : 'testresult',
        component : TestresultComponent
    },
    {
       path : 'testapproval',
       component : TestapprovalComponent
    },
    {
        path : 'registration',
        component : RegistrationComponent
    }
  
];
