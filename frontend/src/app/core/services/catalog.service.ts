import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, of, map, catchError } from 'rxjs';
import { Band, Album, Track } from '../models';
import { environment } from '../../../environments/environment';
import {
  PagedResultDto,
  BandListItemDto,
  BandDetailsDto,
  AlbumDto,
  TrackDto,
  SearchResponseDto,
  FavoriteBandItemDto,
} from '../api/dtos';
import { emojiFor, formatDuration } from '../utils/format';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiBaseUrl;

  /** Lista bandas paginadas (US-09) já marcando as favoritas do usuário. */
  getBands(page = 1, pageSize = 50): Observable<Band[]> {
    return forkJoin({
      paged: this.http.get<PagedResultDto<BandListItemDto>>(
        `${this.api}/bands`,
        { params: { page, pageSize } },
      ),
      favoriteIds: this.favoriteBandIds(),
    }).pipe(
      map(({ paged, favoriteIds }) =>
        paged.items.map((b) => this.toBand(b, favoriteIds.has(b.id))),
      ),
    );
  }

  /** Detalhes da banda (US-10): converte em Band com a contagem real de álbuns. */
  getBandById(id: string): Observable<Band | undefined> {
    return this.http
      .get<BandDetailsDto>(`${this.api}/bands/${id}`)
      .pipe(
        map((d) => ({
          id: d.id,
          name: d.name,
          genre: '',
          emoji: emojiFor(d.id),
          albumCount: d.albums.length,
          coverImageUrl: d.coverImageUrl,
        })),
        catchError(() => of(undefined)),
      );
  }

  /** Álbuns + faixas de uma banda (US-10). */
  getAlbumsByBand(bandId: string): Observable<Album[]> {
    return this.http
      .get<BandDetailsDto>(`${this.api}/bands/${bandId}`)
      .pipe(map((d) => d.albums.map((a) => this.toAlbum(a, d.id, d.name))));
  }

  /** Busca bandas e músicas (US-16). A capitalização é tratada pela API. */
  search(term: string): Observable<{ bands: Band[]; tracks: Track[] }> {
    const q = term.trim();
    if (!q) return of({ bands: [], tracks: [] });
    return this.http
      .get<SearchResponseDto>(`${this.api}/search`, { params: { q } })
      .pipe(
        map((res) => ({
          bands: res.bands.map((b) => ({
            id: b.id,
            name: b.name,
            genre: '',
            emoji: emojiFor(b.id),
            albumCount: 0,
            coverImageUrl: b.coverImageUrl,
          })),
          tracks: res.tracks.map((t) => ({
            id: t.id,
            title: t.title,
            bandName: t.bandName,
            duration: '',
            emoji: emojiFor(t.id),
          })),
        })),
      );
  }

  /** Favorita/desfavorita uma banda (US-15). */
  toggleBandFavorite(bandId: string, isFavorite: boolean): Observable<void> {
    const url = `${this.api}/favorites/bands/${bandId}`;
    const request = isFavorite
      ? this.http.delete(url)
      : this.http.post(url, {});
    return request.pipe(map(() => void 0));
  }

  private favoriteBandIds(): Observable<Set<string>> {
    return this.http
      .get<FavoriteBandItemDto[]>(`${this.api}/favorites/bands`)
      .pipe(
        map((items) => new Set(items.map((i) => i.bandId))),
        catchError(() => of(new Set<string>())),
      );
  }

  private toBand(b: BandListItemDto, favorite: boolean): Band {
    return {
      id: b.id,
      name: b.name,
      genre: '',
      emoji: emojiFor(b.id),
      albumCount: 0,
      coverImageUrl: b.coverImageUrl,
      favorite,
    };
  }

  private toAlbum(a: AlbumDto, bandId: string, bandName: string): Album {
    return {
      id: a.id,
      bandId,
      title: a.title,
      year: a.releaseYear,
      emoji: emojiFor(a.id),
      tracks: a.tracks.map((t) => this.toTrack(t, bandName, a.title)),
    };
  }

  private toTrack(t: TrackDto, bandName: string, albumTitle: string): Track {
    return {
      id: t.id,
      title: t.title,
      bandName,
      albumTitle,
      duration: formatDuration(t.durationSeconds),
      emoji: emojiFor(t.id),
    };
  }
}
