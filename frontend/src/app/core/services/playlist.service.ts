import { Injectable, signal, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, of, map, tap, switchMap, catchError, throwError } from 'rxjs';
import { Playlist, Track } from '../models';
import { environment } from '../../../environments/environment';
import { PlaylistSummaryDto, PlaylistDetailsDto } from '../api/dtos';
import { emojiFor } from '../utils/format';

@Injectable({ providedIn: 'root' })
export class PlaylistService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiBaseUrl;

  private readonly _playlists = signal<Playlist[]>([]);
  readonly playlists = this._playlists.asReadonly();

  /** Carrega as playlists do usuário (US-12 c1). */
  load(): Observable<Playlist[]> {
    return this.http
      .get<PlaylistSummaryDto[]>(`${this.api}/playlists`)
      .pipe(
        map((items) => items.map((p) => this.toSummary(p))),
        tap((list) => this._playlists.set(list)),
        catchError(() => {
          this._playlists.set([]);
          return of<Playlist[]>([]);
        }),
      );
  }

  getPlaylists(): Observable<Playlist[]> {
    return this.load();
  }

  getPlaylistById(id: string): Observable<Playlist | undefined> {
    return this.http
      .get<PlaylistDetailsDto>(`${this.api}/playlists/${id}`)
      .pipe(
        map((d) => this.toDetails(d)),
        catchError(() => of(undefined)),
      );
  }

  /** Cria uma playlist (US-12). Retorna null se o nome for inválido (validação local). */
  createPlaylist(name: string): Observable<Playlist | null> {
    const trimmed = name.trim();
    if (!trimmed || trimmed.length > 100) return of(null);
    return this.http
      .post<{ id: string }>(`${this.api}/playlists`, { name: trimmed })
      .pipe(
        switchMap((created) =>
          this.load().pipe(
            map(
              (list) =>
                list.find((p) => p.id === created.id) ?? {
                  id: created.id,
                  name: trimmed,
                  emoji: emojiFor(created.id),
                  trackCount: 0,
                  trackIds: [],
                  tracks: [],
                },
            ),
          ),
        ),
      );
  }

  /**
   * Adiciona uma faixa à playlist (US-13). Resolve `true` quando adicionada e
   * `false` quando já existia na playlist (409 Conflict).
   */
  addTrack(playlistId: string, track: Track): Observable<boolean> {
    return this.http
      .post<{ trackCount: number }>(
        `${this.api}/playlists/${playlistId}/tracks`,
        { trackId: track.id },
      )
      .pipe(
        tap((res) => this.updateCount(playlistId, res.trackCount)),
        map(() => true),
        catchError((err: HttpErrorResponse) =>
          err.status === 409 ? of(false) : throwError(() => err),
        ),
      );
  }

  /** Remove uma faixa da playlist (US-13). Resolve a quantidade restante. */
  removeTrack(playlistId: string, trackId: string): Observable<number> {
    return this.http
      .delete<{ trackCount: number }>(
        `${this.api}/playlists/${playlistId}/tracks/${trackId}`,
      )
      .pipe(
        tap((res) => this.updateCount(playlistId, res.trackCount)),
        map((res) => res.trackCount),
      );
  }

  private updateCount(playlistId: string, trackCount: number): void {
    this._playlists.update((list) =>
      list.map((p) => (p.id === playlistId ? { ...p, trackCount } : p)),
    );
  }

  private toSummary(p: PlaylistSummaryDto): Playlist {
    return {
      id: p.id,
      name: p.name,
      emoji: emojiFor(p.id),
      trackCount: p.trackCount,
      trackIds: [],
      tracks: [],
    };
  }

  private toDetails(d: PlaylistDetailsDto): Playlist {
    const tracks: Track[] = d.tracks
      .slice()
      .sort((a, b) => a.position - b.position)
      .map((t) => ({
        id: t.trackId,
        title: t.title,
        bandName: t.bandName,
        duration: '',
        emoji: emojiFor(t.trackId),
      }));
    return {
      id: d.id,
      name: d.name,
      emoji: emojiFor(d.id),
      trackCount: d.trackCount,
      trackIds: tracks.map((t) => t.id),
      tracks,
    };
  }
}
