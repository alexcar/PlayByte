import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'pb-forgot-password',
  standalone: true,
  imports: [FormsModule, RouterLink, ButtonModule, InputTextModule, IconFieldModule, InputIconModule, MessageModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss',
})
export class ForgotPasswordComponent {
  private readonly auth = inject(AuthService);

  protected email = '';
  protected readonly loading = signal(false);
  protected readonly sent = signal(false);
  protected readonly invalidEmail = signal(false);

  onSubmit(): void {
    const valid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email.trim());
    if (!valid) {
      this.invalidEmail.set(true);
      return;
    }
    this.invalidEmail.set(false);
    this.loading.set(true);
    // Mesmo response para e-mail existente ou não (proteção contra enumeração).
    this.auth.requestPasswordReset(this.email).subscribe({
      next: () => {
        this.loading.set(false);
        this.sent.set(true);
      },
    });
  }

  resend(): void {
    this.auth.requestPasswordReset(this.email).subscribe();
  }
}
