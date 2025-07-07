import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { UserloginService } from '../../service/AccountService/userLogin/userlogin.service';


@Component({
  selector: 'app-sliderbar',
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  templateUrl: './sliderbar.component.html',
  styleUrls: ['./sliderbar.component.css']
})
export class SliderbarComponent implements OnInit {
userProfile: any;

  
constructor( private userService: UserloginService, private router : Router ) { }

  ngOnInit(): void {   const loginDetails = JSON.parse(localStorage.getItem('loginDetails') || '{}');
   
  console.log('username',loginDetails.userName);
  console.log('password',loginDetails.password);

    this.userService.GetuserLogindetails(loginDetails.userName,loginDetails.password).subscribe(
      (data) => {
        this.userProfile = data;
      },
      (error) => {
        console.error('Error fetching user profile', error);
      }
    );
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
     sessionStorage.removeItem('loggedInUser'); 
     localStorage.removeItem('loginDetails'); 
      this.router.navigate(['']);
  }
}

