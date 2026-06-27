import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { DividerModule } from 'primeng/divider';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'pb-login',
  standalone: true,
  imports: [
    FormsModule, RouterLink, ButtonModule, InputTextModule, PasswordModule,
    DividerModule, IconFieldModule, InputIconModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly messages = inject(MessageService);

  protected email = '';
  protected password = '';
  protected readonly loading = signal(false);

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.messages.add({ severity: 'warn', summary: 'Campos obrigatórios', detail: 'Informe e-mail e senha.' });
      return;
    }
    this.loading.set(true);
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading.set(false);
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/catalogo';
        this.router.navigateByUrl(returnUrl);
      },
      // US-02 c2: mensagem genérica, sem indicar qual campo está incorreto.
      error: () => {
        this.loading.set(false);
        this.messages.add({
          severity: 'error',
          summary: 'Não foi possível entrar',
          detail: 'E-mail ou senha inválidos.',
        });
      },
    });
  }
}
