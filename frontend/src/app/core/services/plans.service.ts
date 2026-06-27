import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { Plan, SubscriptionEvent } from '../models';

@Injectable({ providedIn: 'root' })
export class PlansService {
  private readonly plans: Plan[] = [
    {
      id: 'free',
      name: 'Gratuito',
      priceMonthly: 0,
      priceYearly: 0,
      description: 'Para começar a descobrir música',
      featured: false,
      features: [
        { label: 'Catálogo completo de bandas e álbuns', included: true },
        { label: 'Criar e gerenciar playlists', included: true },
        { label: 'Favoritar músicas e bandas', included: true },
        { label: 'Busca avançada', included: true },
        { label: 'Reprodução de músicas', included: false },
        { label: 'Qualidade de áudio superior', included: false },
        { label: 'Sem anúncios', included: false },
      ],
    },
    {
      id: 'paid',
      name: 'Pago',
      priceMonthly: 19,
      priceYearly: 182,
      description: 'Experiência completa de streaming',
      featured: true,
      features: [
        { label: 'Tudo do plano gratuito', included: true },
        { label: 'Reprodução ilimitada de músicas', included: true, highlight: true },
        { label: 'Qualidade de áudio superior', included: true },
        { label: 'Sem anúncios', included: true },
        { label: 'Suporte prioritário', included: true },
        { label: 'Cancele quando quiser', included: true },
      ],
    },
  ];

  private readonly history: SubscriptionEvent[] = [
    {
      id: 'e1',
      type: 'created',
      title: 'Conta criada',
      description: 'Acesso ao plano gratuito concedido automaticamente',
      date: '2025-01-15',
    },
  ];

  getPlans(): Observable<Plan[]> {
    return of(this.plans).pipe(delay(120));
  }

  getSubscriptionHistory(): Observable<SubscriptionEvent[]> {
    return of(this.history).pipe(delay(120));
  }
}
