import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';


@Component({
  selector: 'app-sliderbar',
  imports: [CommonModule,RouterModule],
  templateUrl: './sliderbar.component.html',
  styleUrls: ['./sliderbar.component.css']
})
export class SliderbarComponent {
  isNavOpen = false;
  activeLink = 0;
  isMasterOpen = false;
  isTransactionOpen = false;
  isFinanceOpen = false;

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

}
