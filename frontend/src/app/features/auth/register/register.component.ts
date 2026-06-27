import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CheckboxModule } from 'primeng/checkbox';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'pb-register',
  standalone: true,
  imports: [
    FormsModule, RouterLink, ButtonModule, InputTextModule, PasswordModule,
    CheckboxModule, IconFieldModule, InputIconModule,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messages = inject(MessageService);

  protected name = '';
  protected email = '';
  protected password = '';
  protected confirmPassword = '';
  protected acceptTerms = false;
  protected readonly loading = signal(false);

  protected readonly perks = [
    { icon: 'pi-list', title: 'Catálogo completo', text: 'Acesse bandas, álbuns e músicas sem precisar pagar.' },
    { icon: 'pi-play-circle', title: 'Playlists ilimitadas', text: 'Crie e organize quantas playlists quiser.' },
    { icon: 'pi-heart', title: 'Favoritos', text: 'Salve músicas e bandas favoritas para acessar sempre.' },
    { icon: 'pi-volume-up', title: 'Reprodução com plano pago', text: 'Faça upgrade a qualquer momento e ouça tudo.' },
  ];

  onSubmit(): void {
    if (!this.name || !this.email || !this.password) {
      this.messages.add({ severity: 'warn', summary: 'Campos obrigatórios', detail: 'Preencha todos os campos.' });
      return;
    }
    if (this.password.length < 8) {
      this.messages.add({ severity: 'warn', summary: 'Senha fraca', detail: 'A senha deve ter no mínimo 8 caracteres.' });
      return;
    }
    if (this.password !== this.confirmPassword) {
      this.messages.add({ severity: 'warn', summary: 'Senhas diferentes', detail: 'As senhas não coincidem.' });
      return;
    }
    if (!this.acceptTerms) {
      this.messages.add({ severity: 'warn', summary: 'Termos', detail: 'Aceite os termos para continuar.' });
      return;
    }
    this.loading.set(true);
    this.auth.register(this.name, this.email, this.password).subscribe({
      next: () => {
        this.loading.set(false);
        this.messages.add({ severity: 'success', summary: 'Conta criada!', detail: 'Bem-vindo ao PlayByte.' });
        this.router.navigate(['/planos']);
      },
      // US-01 c2: e-mail já cadastrado retorna 409; demais erros (400) também são exibidos.
      error: (err) => {
        this.loading.set(false);
        const detail =
          err.status === 409
            ? 'Este e-mail já está em uso.'
            : err.error?.detail ?? 'Não foi possível criar a conta. Verifique os dados e tente novamente.';
        this.messages.add({ severity: 'error', summary: 'Cadastro não concluído', detail });
      },
    });
  }
}
