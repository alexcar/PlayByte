import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, map, switchMap, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SubscribeResponseDto, MySubscriptionResponseDto } from '../api/dtos';

/** Códigos de plano expostos pelo catálogo da API (PlanCatalog no backend). */
export type PlanCode = 'premium-monthly' | 'premium-annual';

@Injectable({ providedIn: 'root' })
export class SubscriptionService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiBaseUrl;

  /**
   * Assina um plano pago (US-05). Em homologação, após criar a assinatura
   * pendente confirmamos o pagamento (endpoint normalmente acionado pelo webhook
   * do gateway) para liberar a reprodução imediatamente.
   */
  subscribe(
    planCode: PlanCode,
    paymentMethod: string = 'credit_card',
  ): Observable<void> {
    return this.http
      .post<SubscribeResponseDto>(`${this.api}/subscriptions`, {
        planCode,
        paymentMethod,
      })
      .pipe(
        switchMap((res) =>
          this.http.post(`${this.api}/payments/${res.paymentId}/approve`, {
            gatewayTransactionId: `homolog-${res.paymentId}`,
          }),
        ),
        map(() => void 0),
      );
  }

  /** Cancela a assinatura ativa (US-08). Acesso mantido até o fim do período pago. */
  cancel(): Observable<void> {
    return this.http
      .delete(`${this.api}/subscriptions`)
      .pipe(map(() => void 0));
  }

  /** Assinatura atual ou null quando o usuário está no plano gratuito (404). */
  getMySubscription(): Observable<MySubscriptionResponseDto | null> {
    return this.http
      .get<MySubscriptionResponseDto>(`${this.api}/subscriptions/me`)
      .pipe(catchError(() => of(null)));
  }
}
