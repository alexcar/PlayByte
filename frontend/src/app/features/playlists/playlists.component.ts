import { Component, inject, signal, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { PlaylistService } from '../../core/services/playlist.service';
import { FavoritesService } from '../../core/services/favorites.service';
import { TrackListComponent } from '../../shared/components/track-list/track-list.component';
import { Playlist, Track } from '../../core/models';

@Component({
  selector: 'pb-playlists',
  standalone: true,
  imports: [FormsModule, RouterLink, ButtonModule, DialogModule, InputTextModule, TrackListComponent],
  templateUrl: './playlists.component.html',
  styleUrl: './playlists.component.scss',
})
export class PlaylistsComponent implements OnInit {
  private readonly playlistService = inject(PlaylistService);
  private readonly favorites = inject(FavoritesService);
  private readonly messages = inject(MessageService);

  protected readonly playlists = this.playlistService.playlists;
  protected readonly selected = signal<Playlist | null>(null);

  protected dialogVisible = false;
  protected newName = '';

  ngOnInit(): void {
    this.playlistService.load().subscribe();
    // Favoritos para marcar os corações das faixas dentro da playlist.
    this.favorites.load().subscribe();
  }

  openDialog(): void {
    this.newName = '';
    this.dialogVisible = true;
  }

  create(): void {
    const name = this.newName.trim();
    // US-12 c2: nome em branco não cria a playlist.
    if (!name) {
      this.messages.add({ severity: 'warn', summary: 'Nome inválido', detail: 'Informe um nome para a playlist.' });
      return;
    }
    // Bloqueia nome duplicado já no cliente (o backend também valida com 409).
    const exists = this.playlists().some((p) => p.name.toLowerCase() === name.toLowerCase());
    if (exists) {
      this.messages.add({ severity: 'warn', summary: 'Nome duplicado', detail: 'Você já tem uma playlist com este nome.' });
      return;
    }
    this.playlistService.createPlaylist(name).subscribe({
      next: (playlist) => {
        if (!playlist) {
          this.messages.add({ severity: 'warn', summary: 'Nome inválido', detail: 'Informe um nome para a playlist.' });
          return;
        }
        this.messages.add({ severity: 'success', summary: 'Playlist criada', detail: `"${playlist.name}" foi criada.` });
        this.dialogVisible = false;
      },
      error: (err: HttpErrorResponse) =>
        err.status === 409
          ? this.messages.add({ severity: 'warn', summary: 'Nome duplicado', detail: 'Você já tem uma playlist com este nome.' })
          : this.messages.add({ severity: 'error', summary: 'Erro', detail: 'Não foi possível criar a playlist.' }),
    });
  }

  open(playlist: Playlist): void {
    // Busca os detalhes (faixas) sob demanda; a listagem traz apenas a contagem.
    this.selected.set(playlist);
    this.playlistService.getPlaylistById(playlist.id).subscribe((detail) => {
      if (detail) this.selected.set({ ...detail, tracks: this.favorites.markTracks(detail.tracks ?? []) });
    });
  }

  /** Remove a faixa da playlist (US-13) e atualiza a tela. */
  removeTrack(track: Track): void {
    const playlist = this.selected();
    if (!playlist) return;
    this.playlistService.removeTrack(playlist.id, track.id).subscribe({
      next: (trackCount) => {
        const tracks = (playlist.tracks ?? []).filter((t) => t.id !== track.id);
        this.selected.set({ ...playlist, tracks, trackCount });
        this.messages.add({ severity: 'success', summary: 'Removida', detail: 'Música removida da playlist.' });
      },
      error: () =>
        this.messages.add({ severity: 'error', summary: 'Erro', detail: 'Não foi possível remover a música.' }),
    });
  }

  back(): void {
    this.selected.set(null);
  }
}
