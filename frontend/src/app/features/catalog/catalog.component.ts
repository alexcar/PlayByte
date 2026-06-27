import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { toSignal } from '@angular/core/rxjs-interop';
import { CatalogService } from '../../core/services/catalog.service';
import { AuthService } from '../../core/services/auth.service';
import { FavoritesService } from '../../core/services/favorites.service';
import { BandCardComponent } from '../../shared/components/band-card/band-card.component';
import { UpgradeBannerComponent } from '../../shared/components/upgrade-banner/upgrade-banner.component';
import { TrackListComponent } from '../../shared/components/track-list/track-list.component';
import { Band, Album } from '../../core/models';

@Component({
  selector: 'pb-catalog',
  standalone: true,
  imports: [
    FormsModule, ButtonModule, DrawerModule, PaginatorModule,
    BandCardComponent, UpgradeBannerComponent, TrackListComponent,
  ],
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss',
})
export class CatalogComponent implements OnInit {
  private readonly catalog = inject(CatalogService);
  protected readonly auth = inject(AuthService);
  private readonly favorites = inject(FavoritesService);

  protected readonly bands = toSignal(this.catalog.getBands(), { initialValue: [] });
  // A API de catálogo não classifica bandas por gênero; mantemos apenas "Todos".
  protected readonly genres = ['Todos'];
  protected readonly activeGenre = signal('Todos');

  // Paginação client-side: 5 bandas por página.
  protected readonly pageSize = 5;
  protected readonly first = signal(0);

  protected readonly drawerVisible = signal(false);
  protected readonly selectedBand = signal<Band | null>(null);
  protected readonly selectedAlbums = signal<Album[]>([]);

  protected readonly filteredBands = computed(() => {
    const g = this.activeGenre();
    if (g === 'Todos') return this.bands();
    return this.bands().filter((b) => b.genre.toLowerCase().includes(g.toLowerCase()));
  });

  protected readonly totalRecords = computed(() => this.filteredBands().length);

  // Apenas as bandas da página atual (5 por vez).
  protected readonly pagedBands = computed(() =>
    this.filteredBands().slice(this.first(), this.first() + this.pageSize),
  );

  ngOnInit(): void {
    // Carrega os favoritos para marcar os corações das faixas ao abrir um álbum.
    this.favorites.load().subscribe();
  }

  setGenre(g: string): void {
    this.activeGenre.set(g);
    this.first.set(0); // volta para a primeira página ao trocar o filtro
  }

  onPageChange(event: PaginatorState): void {
    this.first.set(event.first ?? 0);
  }

  openBand(band: Band): void {
    this.selectedBand.set(band);
    this.drawerVisible.set(true);
    this.catalog.getAlbumsByBand(band.id).subscribe((albums) =>
      // Marca as faixas favoritas para o coração aparecer vermelho (US-14).
      this.selectedAlbums.set(
        albums.map((a) => ({ ...a, tracks: this.favorites.markTracks(a.tracks) })),
      ),
    );
  }

  onFavoriteToggle(band: Band): void {
    const wasFavorite = !!band.favorite;
    band.favorite = !wasFavorite; // feedback visual imediato (US-15 c1/c2)
    this.catalog.toggleBandFavorite(band.id, wasFavorite).subscribe({
      error: () => (band.favorite = wasFavorite), // reverte em caso de falha
    });
  }
}
