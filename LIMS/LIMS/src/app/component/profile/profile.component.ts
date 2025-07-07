import { Component, inject, OnInit } from '@angular/core';
import { SliderbarComponent } from "../sliderbar/sliderbar.component";
import { CommonModule } from '@angular/common';
import { userregistration } from '../../modal/AccountModal/UserRegistrationModal/userregistraionmodal';
import { UserloginService } from '../../service/AccountService/userLogin/userlogin.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-profile',
  imports: [SliderbarComponent, CommonModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  userProfile: any;
  // userName: string = '';
  // password: string = '';
  
    constructor(
        private userService: UserloginService
      ) { }

  ngOnInit(): void {
   const loginDetails = JSON.parse(localStorage.getItem('loginDetails') || '{}');
   
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

}

