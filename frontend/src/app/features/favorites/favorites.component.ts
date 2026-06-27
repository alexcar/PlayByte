import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { FavoritesService } from '../../core/services/favorites.service';
import { TrackListComponent } from '../../shared/components/track-list/track-list.component';

@Component({
  selector: 'pb-favorites',
  standalone: true,
  imports: [RouterLink, ButtonModule, TabsModule, TrackListComponent],
  templateUrl: './favorites.component.html',
  styleUrl: './favorites.component.scss',
})
export class FavoritesComponent implements OnInit {
  protected readonly favorites = inject(FavoritesService);
  protected readonly activeTab = signal('tracks');

  ngOnInit(): void {
    // Carrega favoritos persistidos (US-14 c4 / US-15 c4).
    this.favorites.load().subscribe();
  }

  removeBand(id: string): void {
    this.favorites.removeBand(id);
  }
}
