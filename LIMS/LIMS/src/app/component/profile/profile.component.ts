import { Component, inject, OnInit } from '@angular/core';
import { SliderbarComponent } from "../sliderbar/sliderbar.component";
import { CommonModule } from '@angular/common';
import { userregistration } from '../../modal/AccountModal/UserRegistrationModal/userregistraionmodal';
import { UserloginService } from '../../service/AccountService/userLogin/userlogin.service';

@Component({
  selector: 'app-profile',
  imports: [SliderbarComponent, CommonModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {

  userregistration = {
    "userName": "",
    "password": "",
    "email": "",
    "phoneNumber": "",
  }

  detaisList: userregistration[] = [];

  userlogin = inject(UserloginService);

  ngOnInit(): void {
    this.userlogin.GetuserLogindetails().subscribe((response) => {
      this.detaisList = response;
    },
  )}
  
}

