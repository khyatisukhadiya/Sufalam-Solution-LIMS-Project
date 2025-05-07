import { Component } from '@angular/core';
import { SliderbarComponent } from '../../component/sliderbar/sliderbar.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-service',
  imports: [SliderbarComponent, CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './service.component.html',
  styleUrl: './service.component.css'
})
export class ServiceComponent {

}
