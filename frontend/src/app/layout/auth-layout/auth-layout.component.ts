import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'pb-auth-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <div class="pb-auth-navbar">
      <a routerLink="/inicio" class="pb-logo">Play<span>Byte</span></a>
    </div>
    <router-outlet />
  `,
  styles: [
    `
      .pb-auth-navbar {
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        padding: 1.25rem 2rem;
        z-index: 10;
      }
      .pb-logo {
        font-family: var(--pb-font-display);
        font-weight: 800;
        font-size: 1.5rem;
        color: var(--pb-primary);
      }
      .pb-logo span {
        color: var(--pb-text);
      }
    `,
  ],
})
export class AuthLayoutComponent {}
