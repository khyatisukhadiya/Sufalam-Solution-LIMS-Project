import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { UserloginService } from '../../service/AccountService/userLogin/userlogin.service';
import { userregistration } from '../../modal/AccountModal/UserRegistrationModal/userregistraionmodal';


@Component({
  selector: 'app-sliderbar',
  imports: [CommonModule, RouterModule,FormsModule,ReactiveFormsModule],
  templateUrl: './sliderbar.component.html',
  styleUrls: ['./sliderbar.component.css']
})
export class SliderbarComponent implements OnInit {


  detailsList : userregistration[] = [];
  router: any;


  ngOnInit(): void {

  }

 loginFrom : FormGroup = new FormGroup({});

 userloginService = inject(UserloginService);

  isNavOpen = false;
  activeLink = 0;
  isMasterOpen = false;
  isTransactionOpen = false;
  isFinanceOpen = false;
  isShowcard = false;

  toggleNavbar(): void {
    this.isNavOpen = !this.isNavOpen;
  }


  toggleMasterMenu(): void {
    this.isMasterOpen = !this.isMasterOpen;
  }

  toggleTransactionMenu(): void {
    this.isTransactionOpen = !this.isTransactionOpen;
  }

  toggleFinanceMenu(): void {
    this.isFinanceOpen = !this.isFinanceOpen;
  }


  setActiveLink(index: number): void {
    this.activeLink = index;
  }


  toggleProfile() : void{
    this.isShowcard = !this.isShowcard;
  }

   signOut(): void {
    sessionStorage.clear();  
    localStorage.clear();    

    this.router.navigate(['/login']);
  }
}

