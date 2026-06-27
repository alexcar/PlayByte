import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'pb-upgrade-banner',
  standalone: true,
  imports: [RouterLink, ButtonModule],
  template: `
    <div class="pb-upgrade">
      <i class="pi pi-info-circle"></i>
      <span class="pb-upgrade-text">
        Você está no <strong>plano gratuito</strong>. Explore o catálogo livremente, mas para ouvir
        músicas você precisa do <a routerLink="/planos">plano pago</a>.
      </span>
      <p-button label="Assinar" routerLink="/planos" styleClass="pb-pill" size="small" />
    </div>
  `,
  styles: [
    `
      .pb-upgrade {
        background: rgba(29, 185, 84, 0.08);
        border: 1px solid rgba(29, 185, 84, 0.2);
        border-radius: 12px;
        padding: 1rem 1.25rem;
        display: flex;
        align-items: center;
        gap: 1rem;
        margin-bottom: 1.5rem;
      }
      i {
        color: var(--pb-primary);
        font-size: 1.25rem;
      }
      .pb-upgrade-text {
        flex: 1;
        font-size: 0.875rem;
      }
      a {
        color: var(--pb-primary);
        font-weight: 600;
      }
    `,
  ],
})
export class UpgradeBannerComponent {}
