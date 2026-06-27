import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { TimelineModule } from 'primeng/timeline';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { SubscriptionService } from '../../core/services/subscription.service';
import { FavoritesService } from '../../core/services/favorites.service';
import { PlaylistService } from '../../core/services/playlist.service';
import { MySubscriptionResponseDto } from '../../core/api/dtos';
import { SubscriptionEvent } from '../../core/models';
import { formatMonthYear, formatDateBr } from '../../core/utils/format';

@Component({
  selector: 'pb-profile',
  standalone: true,
  imports: [
    FormsModule, ButtonModule, AvatarModule, DialogModule, InputTextModule,
    PasswordModule, ToggleSwitchModule, TimelineModule, ConfirmDialogModule,
  ],
  providers: [ConfirmationService],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly subscriptions = inject(SubscriptionService);
  private readonly favorites = inject(FavoritesService);
  private readonly playlistService = inject(PlaylistService);
  private readonly router = inject(Router);
  private readonly messages = inject(MessageService);
  private readonly confirm = inject(ConfirmationService);

  // Contadores reais (favoritos, bandas seguidas, playlists).
  protected readonly favTrackCount = this.favorites.trackCount;
  protected readonly favBandCount = this.favorites.bandCount;
  protected readonly playlistCount = computed(() => this.playlistService.playlists().length);

  // Assinatura atual (para data da próxima cobrança).
  protected readonly subscription = signal<MySubscriptionResponseDto | null>(null);
  protected readonly nextBilling = computed(() => formatDateBr(this.subscription()?.currentPeriodEnd));

  // "Membro desde" derivado da data real de criação da conta.
  protected readonly memberSince = computed(() => formatMonthYear(this.auth.user()?.memberSince));

  // Linha do tempo baseada em dados reais.
  protected readonly history = computed<SubscriptionEvent[]>(() => {
    const events: SubscriptionEvent[] = [
      {
        id: 'created',
        type: 'created',
        title: 'Conta criada',
        description: 'Acesso ao plano gratuito concedido automaticamente',
        date: formatDateBr(this.auth.user()?.memberSince),
      },
    ];
    const sub = this.subscription();
    if (sub?.hasActiveAccess) {
      events.unshift({
        id: 'subscription',
        type: 'charge',
        title: `Assinatura ${sub.planName}`,
        description: 'Plano pago ativo',
        date: `Vigente até ${formatDateBr(sub.currentPeriodEnd)}`,
      });
    }
    return events;
  });

  protected readonly stats = computed(() => [
    { icon: 'pi-music', label: 'Músicas favoritas', value: String(this.favTrackCount()) },
    { icon: 'pi-users', label: 'Bandas seguidas', value: String(this.favBandCount()) },
    { icon: 'pi-play-circle', label: 'Playlists', value: String(this.playlistCount()) },
    { icon: 'pi-clock', label: 'Membro desde', value: this.memberSince() },
  ]);

  protected editVisible = false;
  protected passwordVisible = false;
  protected editName = this.auth.user()?.name ?? '';
  protected editEmail = this.auth.user()?.email ?? '';
  protected currentPassword = '';
  protected newPassword = '';
  protected saving = false;

  // Preferências de notificação
  protected readonly notifContract = signal(true);
  protected readonly notifRenewal = signal(true);
  protected readonly notifCancel = signal(true);
  protected readonly notifNews = signal(false);

  ngOnInit(): void {
    // Carrega contadores e assinatura reais.
    this.favorites.load().subscribe();
    this.playlistService.load().subscribe();
    this.subscriptions.getMySubscription().subscribe((sub) => this.subscription.set(sub));
  }

  openEdit(): void {
    // Reidrata os campos com os valores atuais ao abrir o diálogo.
    this.editName = this.auth.user()?.name ?? '';
    this.editEmail = this.auth.user()?.email ?? '';
    this.editVisible = true;
  }

  saveProfile(): void {
    const name = this.editName.trim();
    const email = this.editEmail.trim();
    if (!name || !email) {
      this.messages.add({ severity: 'warn', summary: 'Dados inválidos', detail: 'Nome e e-mail são obrigatórios.' });
      return;
    }
    this.saving = true;
    this.auth.updateProfile(name, email).subscribe({
      next: () => {
        this.saving = false;
        this.editVisible = false;
        this.messages.add({ severity: 'success', summary: 'Perfil atualizado', detail: 'Suas informações foram salvas.' });
      },
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        const detail = err.status === 409
          ? 'Já existe um usuário com este e-mail.'
          : 'Não foi possível salvar as alterações.';
        this.messages.add({ severity: 'error', summary: 'Erro', detail });
      },
    });
  }

  savePassword(): void {
    if (!this.currentPassword) {
      this.messages.add({ severity: 'warn', summary: 'Senha atual', detail: 'Informe a senha atual.' });
      return;
    }
    if (this.newPassword.length < 8) {
      this.messages.add({ severity: 'warn', summary: 'Senha fraca', detail: 'Mínimo de 8 caracteres.' });
      return;
    }
    this.saving = true;
    this.auth.changePassword(this.currentPassword, this.newPassword).subscribe({
      next: () => {
        this.saving = false;
        this.passwordVisible = false;
        this.currentPassword = '';
        this.newPassword = '';
        this.messages.add({ severity: 'success', summary: 'Senha alterada', detail: 'Sua senha foi atualizada.' });
      },
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        // 400 cobre senha atual incorreta e nova senha fraca (regras do backend).
        const detail = err.error?.detail ?? 'Não foi possível alterar a senha.';
        this.messages.add({ severity: 'error', summary: 'Erro', detail });
      },
    });
  }

  /** US-05: assina o plano pago (mensal) e libera a reprodução imediatamente. */
  upgrade(): void {
    this.subscriptions.subscribe('premium-monthly').subscribe({
      next: () => {
        this.auth.refreshSubscription().subscribe();
        this.subscriptions.getMySubscription().subscribe((sub) => this.subscription.set(sub));
        this.messages.add({ severity: 'success', summary: 'Bem-vindo ao plano pago!', detail: 'Você já pode reproduzir músicas.' });
      },
      // US-05 c4: pagamento recusado mantém o usuário no plano gratuito.
      error: () =>
        this.messages.add({ severity: 'error', summary: 'Pagamento não aprovado', detail: 'Não foi possível concluir a assinatura. Você continua no plano gratuito.' }),
    });
  }

  confirmCancelPlan(): void {
    this.confirm.confirm({
      header: 'Cancelar assinatura',
      message: 'Tem certeza que deseja cancelar o plano pago? Você manterá o acesso até o fim do período já pago.',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, cancelar',
      rejectLabel: 'Voltar',
      accept: () => {
        // US-08: cancela a assinatura ativa.
        this.subscriptions.cancel().subscribe({
          next: () => {
            this.auth.refreshSubscription().subscribe();
            this.subscriptions.getMySubscription().subscribe((sub) => this.subscription.set(sub));
            this.messages.add({ severity: 'info', summary: 'Assinatura cancelada', detail: 'Um e-mail de confirmação foi enviado.' });
          },
          error: () =>
            this.messages.add({ severity: 'error', summary: 'Erro', detail: 'Não foi possível cancelar a assinatura.' }),
        });
      },
    });
  }

  logout(): void {
    this.confirm.confirm({
      header: 'Sair da conta',
      message: 'Deseja realmente sair?',
      icon: 'pi pi-sign-out',
      acceptLabel: 'Sair',
      rejectLabel: 'Cancelar',
      accept: () => {
        this.auth.logout();
        this.router.navigate(['/login']);
      },
    });
  }
}
