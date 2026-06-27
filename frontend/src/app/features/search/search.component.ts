import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CatalogService } from '../../core/services/catalog.service';
import { FavoritesService } from '../../core/services/favorites.service';
import { TrackListComponent } from '../../shared/components/track-list/track-list.component';
import { BandCardComponent } from '../../shared/components/band-card/band-card.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { Band, Track, Album } from '../../core/models';

@Component({
  selector: 'pb-search',
  standalone: true,
  imports: [
    FormsModule, ButtonModule, DrawerModule, IconFieldModule, InputIconModule, InputTextModule,
    ProgressSpinnerModule, TrackListComponent, BandCardComponent, EmptyStateComponent,
  ],
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss',
})
export class SearchComponent implements OnInit {
  private readonly catalog = inject(CatalogService);
  private readonly favorites = inject(FavoritesService);

  protected term = '';
  protected readonly searched = signal(false);
  protected readonly loading = signal(false);
  protected readonly elapsedMs = signal(0);
  protected readonly resultBands = signal<Band[]>([]);
  protected readonly resultTracks = signal<Track[]>([]);

  protected readonly drawerVisible = signal(false);
  protected readonly selectedBand = signal<Band | null>(null);
  protected readonly selectedAlbums = signal<Album[]>([]);

  protected readonly suggestions = ['Queen', 'Rock', 'Pink Floyd', 'Coldplay', 'Metallica'];

  ngOnInit(): void {
    // Favoritos carregados para marcar os corações (faixas e bandas) nos resultados.
    this.favorites.load().subscribe();
  }

  search(): void {
    const q = this.term.trim();
    if (!q) return;
    this.loading.set(true);
    const start = performance.now();
    this.catalog.search(q).subscribe((res) => {
      this.elapsedMs.set(Math.round(performance.now() - start));
      this.resultBands.set(this.favorites.markBands(res.bands));
      this.resultTracks.set(this.favorites.markTracks(res.tracks));
      this.loading.set(false);
      this.searched.set(true);
    });
  }

  quick(term: string): void {
    this.term = term;
    this.search();
  }

  /** Favorita/desfavorita a banda do resultado (US-15) mantendo o coração vermelho. */
  onFavoriteToggle(band: Band): void {
    band.favorite = !band.favorite; // feedback imediato
    this.favorites.toggleBand(band);
  }

  /** Abre o detalhe da banda (álbuns/faixas) ao clicar no card (US-10). */
  openBand(band: Band): void {
    this.selectedBand.set(band);
    this.drawerVisible.set(true);
    this.catalog.getAlbumsByBand(band.id).subscribe((albums) =>
      this.selectedAlbums.set(
        albums.map((a) => ({ ...a, tracks: this.favorites.markTracks(a.tracks) })),
      ),
    );
  }

  get hasResults(): boolean {
    return this.resultBands().length > 0 || this.resultTracks().length > 0;
  }
}
