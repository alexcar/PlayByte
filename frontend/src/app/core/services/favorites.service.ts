import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, of, map, tap, catchError } from 'rxjs';
import { Track, Band } from '../models';
import { environment } from '../../../environments/environment';
import { FavoriteTrackItemDto, FavoriteBandItemDto } from '../api/dtos';
import { emojiFor } from '../utils/format';

@Injectable({ providedIn: 'root' })
export class FavoritesService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiBaseUrl;

  private readonly _favTracks = signal<Track[]>([]);
  private readonly _favBands = signal<Band[]>([]);

  readonly favTracks = this._favTracks.asReadonly();
  readonly favBands = this._favBands.asReadonly();
  readonly trackCount = computed(() => this._favTracks().length);
  readonly bandCount = computed(() => this._favBands().length);

  /** Conjuntos de ids favoritos para cruzamento rápido (coração vermelho). */
  readonly favoriteTrackIds = computed(() => new Set(this._favTracks().map((t) => t.id)));
  readonly favoriteBandIds = computed(() => new Set(this._favBands().map((b) => b.id)));

  /** Marca o flag `favorite` numa lista de faixas a partir dos favoritos carregados. */
  markTracks(tracks: Track[]): Track[] {
    const ids = this.favoriteTrackIds();
    return tracks.map((t) => ({ ...t, favorite: ids.has(t.id) }));
  }

  /** Marca o flag `favorite` numa lista de bandas a partir dos favoritos carregados. */
  markBands(bands: Band[]): Band[] {
    const ids = this.favoriteBandIds();
    return bands.map((b) => ({ ...b, favorite: ids.has(b.id) }));
  }

  /** Carrega favoritos do usuário (US-14/US-15, persistidos entre sessões). */
  load(): Observable<void> {
    return forkJoin({
      tracks: this.http
        .get<FavoriteTrackItemDto[]>(`${this.api}/favorites/tracks`)
        .pipe(catchError(() => of<FavoriteTrackItemDto[]>([]))),
      bands: this.http
        .get<FavoriteBandItemDto[]>(`${this.api}/favorites/bands`)
        .pipe(catchError(() => of<FavoriteBandItemDto[]>([]))),
    }).pipe(
      tap(({ tracks, bands }) => {
        this._favTracks.set(tracks.map((t) => this.toTrack(t)));
        this._favBands.set(bands.map((b) => this.toBand(b)));
      }),
      map(() => void 0),
    );
  }

  getFavoriteTracks(): Observable<Track[]> {
    return this.load().pipe(map(() => this._favTracks()));
  }

  getFavoriteBands(): Observable<Band[]> {
    return this.load().pipe(map(() => this._favBands()));
  }

  removeTrack(id: string): void {
    this.http.delete(`${this.api}/favorites/tracks/${id}`).subscribe({
      next: () => this._favTracks.update((list) => list.filter((t) => t.id !== id)),
    });
  }

  removeBand(id: string): void {
    this.http.delete(`${this.api}/favorites/bands/${id}`).subscribe({
      next: () => this._favBands.update((list) => list.filter((b) => b.id !== id)),
    });
  }

  /** Alterna o favorito de uma música (US-14 c1/c2). */
  toggleTrack(track: Track): void {
    const exists = this._favTracks().some((t) => t.id === track.id);
    if (exists) {
      this.removeTrack(track.id);
      return;
    }
    this.http.post(`${this.api}/favorites/tracks/${track.id}`, {}).subscribe({
      next: () =>
        this._favTracks.update((list) => [...list, { ...track, favorite: true }]),
    });
  }

  /** Alterna o favorito de uma banda (US-15 c1/c2), mantendo o signal sincronizado. */
  toggleBand(band: Band): void {
    const exists = this._favBands().some((b) => b.id === band.id);
    if (exists) {
      this.removeBand(band.id);
      return;
    }
    this.http.post(`${this.api}/favorites/bands/${band.id}`, {}).subscribe({
      next: () =>
        this._favBands.update((list) => [...list, { ...band, favorite: true }]),
    });
  }

  private toTrack(t: FavoriteTrackItemDto): Track {
    return {
      id: t.trackId,
      title: t.title,
      bandName: t.bandName,
      duration: '',
      emoji: emojiFor(t.trackId),
      favorite: true,
    };
  }

  private toBand(b: FavoriteBandItemDto): Band {
    return {
      id: b.bandId,
      name: b.name,
      genre: '',
      emoji: emojiFor(b.bandId),
      albumCount: 0,
      coverImageUrl: b.coverImageUrl,
      favorite: true,
    };
  }
}
