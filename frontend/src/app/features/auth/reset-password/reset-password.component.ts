import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'pb-reset-password',
  standalone: true,
  imports: [FormsModule, RouterLink, ButtonModule, PasswordModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly messages = inject(MessageService);

  protected password = '';
  protected confirmPassword = '';
  protected token = '';
  protected readonly loading = signal(false);
  protected readonly done = signal(false);
  // Estado do link: 'valid' | 'expired'. Lido de ?expired=1 ou de falha no token.
  protected readonly linkState = signal<'valid' | 'expired'>('valid');

  protected readonly remaining = signal(29 * 60 + 47);
  private timerId?: ReturnType<typeof setInterval>;

  protected readonly countdown = computed(() => {
    const s = this.remaining();
    const m = Math.floor(s / 60);
    const sec = s % 60;
    return `${m.toString().padStart(2, '0')}:${sec.toString().padStart(2, '0')}`;
  });

  protected readonly passwordsMatch = computed(
    () => this.confirmPassword.length > 0 && this.password === this.confirmPassword,
  );

  ngOnInit(): void {
    // Token recebido por e-mail (?token=...). US-03: o link carrega o token de redefinição.
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    const expired = this.route.snapshot.queryParamMap.get('expired');
    if (expired === '1') {
      this.linkState.set('expired');
      return;
    }
    this.timerId = setInterval(() => {
      this.remaining.update((v) => (v > 0 ? v - 1 : 0));
      if (this.remaining() === 0) {
        this.linkState.set('expired');
        this.stopTimer();
      }
    }, 1000);
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  private stopTimer(): void {
    if (this.timerId) clearInterval(this.timerId);
  }

  onSubmit(): void {
    if (this.password.length < 8) {
      this.messages.add({ severity: 'warn', summary: 'Senha fraca', detail: 'Mínimo de 8 caracteres.' });
      return;
    }
    if (!this.passwordsMatch()) {
      this.messages.add({ severity: 'warn', summary: 'Senhas diferentes', detail: 'As senhas não coincidem.' });
      return;
    }
    this.loading.set(true);
    this.auth.resetPassword(this.token, this.password).subscribe({
      next: () => {
        this.loading.set(false);
        this.stopTimer();
        this.done.set(true);
      },
      // US-03 c6: token inválido/expirado → orienta solicitar novo link.
      error: () => {
        this.loading.set(false);
        this.stopTimer();
        this.linkState.set('expired');
      },
    });
  }
}
