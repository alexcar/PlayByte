import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SelectButtonModule } from 'primeng/selectbutton';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { AccordionModule } from 'primeng/accordion';
import { MessageService } from 'primeng/api';
import { toSignal } from '@angular/core/rxjs-interop';
import { PlansService } from '../../core/services/plans.service';
import { AuthService } from '../../core/services/auth.service';
import { SubscriptionService } from '../../core/services/subscription.service';

@Component({
  selector: 'pb-plans',
  standalone: true,
  imports: [FormsModule, ButtonModule, SelectButtonModule, TableModule, AccordionModule],
  templateUrl: './plans.component.html',
  styleUrl: './plans.component.scss',
})
export class PlansComponent {
  private readonly plansService = inject(PlansService);
  protected readonly auth = inject(AuthService);
  private readonly subscriptions = inject(SubscriptionService);
  private readonly router = inject(Router);
  private readonly messages = inject(MessageService);
  protected readonly plans = toSignal(this.plansService.getPlans(), { initialValue: [] });
  protected readonly subscribing = signal(false);

  protected billing = 'monthly';
  protected readonly billingOptions = [
    { label: 'Mensal', value: 'monthly' },
    { label: 'Anual (-20%)', value: 'yearly' },
  ];

  protected readonly comparison = [
    { feature: 'Catálogo de bandas e álbuns', free: true, paid: true },
    { feature: 'Criar playlists', free: true, paid: true },
    { feature: 'Favoritar músicas e bandas', free: true, paid: true },
    { feature: 'Busca de bandas e músicas', free: true, paid: true },
    { feature: 'Reprodução de músicas', free: false, paid: true },
    { feature: 'Qualidade de áudio superior', free: false, paid: true },
    { feature: 'Sem anúncios', free: false, paid: true },
  ];

  protected readonly faqs = [
    { q: 'Posso cancelar a qualquer momento?', a: 'Sim. O cancelamento pode ser feito a qualquer momento pela área de perfil. O acesso ao plano pago é mantido até o fim do período já pago.' },
    { q: 'O plano gratuito tem prazo?', a: 'Não. O plano gratuito é para sempre — sem prazo de expiração e sem necessidade de cartão de crédito.' },
    { q: 'Como funciona a cobrança?', a: 'A cobrança é automática no cartão informado, mensal ou anualmente conforme o plano escolhido. Você recebe um e-mail de confirmação a cada renovação.' },
    { q: 'Minha playlist fica salva se eu cancelar?', a: 'Sim. Todas as suas playlists e favoritos ficam salvos independentemente do plano. Apenas a reprodução de músicas fica indisponível no plano gratuito.' },
  ];

  paidPrice(): string {
    // Preços do PlanCatalog do backend: mensal R$19,90; anual R$199 (≈ R$16,58/mês).
    return this.billing === 'monthly' ? '19,90' : '16,58';
  }

  /** CTA do plano pago: visitante vai ao cadastro; autenticado assina (US-05). */
  onPaidCta(): void {
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/cadastro']);
      return;
    }
    if (this.auth.isPaid()) {
      this.router.navigate(['/perfil']);
      return;
    }
    const planCode = this.billing === 'monthly' ? 'premium-monthly' : 'premium-annual';
    this.subscribing.set(true);
    this.subscriptions.subscribe(planCode).subscribe({
      next: () => {
        this.subscribing.set(false);
        this.auth.refreshSubscription().subscribe();
        this.messages.add({ severity: 'success', summary: 'Plano ativado!', detail: 'Você já pode reproduzir músicas.' });
        this.router.navigate(['/catalogo']);
      },
      // US-05 c4: pagamento recusado mantém o plano gratuito.
      error: () => {
        this.subscribing.set(false);
        this.messages.add({ severity: 'error', summary: 'Pagamento não aprovado', detail: 'Não foi possível concluir a assinatura.' });
      },
    });
  }

  /** CTA do plano gratuito: visitante cadastra; autenticado vai ao catálogo. */
  onFreeCta(): void {
    this.router.navigate([this.auth.isAuthenticated() ? '/catalogo' : '/cadastro']);
  }
}
