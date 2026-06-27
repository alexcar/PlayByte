import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';
import { PlayerService } from '../../../core/services/player.service';
import { FavoritesService } from '../../../core/services/favorites.service';
import { PlaylistService } from '../../../core/services/playlist.service';
import { Track } from '../../../core/models';

@Component({
  selector: 'pb-track-list',
  standalone: true,
  imports: [ButtonModule, DialogModule],
  templateUrl: './track-list.component.html',
  styleUrl: './track-list.component.scss',
})
export class TrackListComponent {
  @Input({ required: true }) tracks: Track[] = [];
  @Input() showIndex = true;
  /** Em uma playlist, mostra "-" (remover) no lugar de "+" (adicionar). */
  @Input() removable = false;
  @Output() trackRemoved = new EventEmitter<Track>();

  private readonly player = inject(PlayerService);
  private readonly favorites = inject(FavoritesService);
  protected readonly playlistService = inject(PlaylistService);
  private readonly messages = inject(MessageService);

  protected readonly playlists = this.playlistService.playlists;
  protected readonly pickerVisible = signal(false);
  protected readonly pickerTrack = signal<Track | null>(null);

  onPlay(track: Track): void {
    this.player.play(track);
  }

  onToggleFav(track: Track, event: Event): void {
    event.stopPropagation();
    this.favorites.toggleTrack(track);
    track.favorite = !track.favorite;
  }

  /** Emite a remoção da faixa para o componente pai tratar (US-13). */
  onRemove(track: Track, event: Event): void {
    event.stopPropagation();
    this.trackRemoved.emit(track);
  }

  /** Abre o seletor de playlists para adicionar a faixa (US-13 c1). */
  openPicker(track: Track, event: Event): void {
    event.stopPropagation();
    this.pickerTrack.set(track);
    this.pickerVisible.set(true);
    this.playlistService.load().subscribe();
  }

  /** Adiciona a faixa na playlist escolhida (US-13 c2/c3). */
  addToPlaylist(playlistId: string): void {
    const track = this.pickerTrack();
    if (!track) return;
    this.playlistService.addTrack(playlistId, track).subscribe({
      next: (added) => {
        this.pickerVisible.set(false);
        this.messages.add(
          added
            ? { severity: 'success', summary: 'Adicionada', detail: 'Música adicionada à playlist.' }
            : { severity: 'info', summary: 'Já está na playlist', detail: 'Esta música já está nesta playlist.' },
        );
      },
      error: () =>
        this.messages.add({ severity: 'error', summary: 'Erro', detail: 'Não foi possível adicionar à playlist.' }),
    });
  }
}
