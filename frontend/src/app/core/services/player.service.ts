import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MessageService } from 'primeng/api';
import { AuthService } from './auth.service';
import { Track } from '../models';
import { environment } from '../../../environments/environment';
import { PlaybackResponseDto } from '../api/dtos';

@Injectable({ providedIn: 'root' })
export class PlayerService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly messages = inject(MessageService);
  private readonly api = environment.apiBaseUrl;

  private readonly _current = signal<Track | null>(null);
  private readonly _playing = signal(false);
  private readonly _progress = signal(0);

  readonly current = this._current.asReadonly();
  readonly playing = this._playing.asReadonly();
  readonly progress = this._progress.asReadonly();
  readonly hasTrack = computed(() => this._current() !== null);

  /**
   * Reproduz uma música via API (US-11). O plano gratuito é bloqueado com convite
   * ao upgrade (US-11 critério 2); o servidor também rejeita com 403.
   */
  play(track: Track): boolean {
    if (!this.auth.isPaid()) {
      this.inviteToUpgrade();
      return false;
    }

    this.http
      .post<PlaybackResponseDto>(`${this.api}/tracks/${track.id}/play`, {})
      .subscribe({
        next: (res) => {
          this._current.set({ ...track, streamUrl: res.streamUrl });
          this._playing.set(true);
          this._progress.set(0);
        },
        error: (err) => {
          if (err.status === 403) {
            this.inviteToUpgrade();
          } else {
            this.messages.add({
              severity: 'error',
              summary: 'Não foi possível reproduzir',
              detail: 'Tente novamente em instantes.',
              life: 4000,
            });
          }
        },
      });

    return true;
  }

  togglePlay(): void {
    if (!this.auth.isPaid()) return;
    this._playing.update((p) => !p);
  }

  setProgress(value: number): void {
    this._progress.set(Math.min(100, Math.max(0, value)));
  }

  private inviteToUpgrade(): void {
    this.messages.add({
      severity: 'info',
      summary: 'Ouça sem limites',
      detail: 'A reprodução de músicas é exclusiva do plano pago. Assine para ouvir.',
      life: 4000,
    });
  }
}
