import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Band } from '../../../core/models';

@Component({
  selector: 'pb-band-card',
  standalone: true,
  template: `
    <div class="pb-band-card" (click)="select.emit(band)">
      <div class="pb-band-thumb">
        {{ band.emoji }}
        <button class="pb-band-fav" (click)="toggleFav($event)">
          <i class="pi" [class.pi-heart-fill]="band.favorite" [class.pi-heart]="!band.favorite"
             [style.color]="band.favorite ? 'var(--pb-danger)' : 'var(--pb-muted)'"></i>
        </button>
      </div>
      <div class="pb-band-info">
        <div class="pb-band-name">{{ band.name }}</div>
        <div class="pb-band-meta">{{ band.genre || 'Banda' }}@if (band.albumCount) { · {{ band.albumCount }} álbuns }</div>
      </div>
    </div>
  `,
  styleUrl: './band-card.component.scss',
})
export class BandCardComponent {
  @Input({ required: true }) band!: Band;
  @Output() select = new EventEmitter<Band>();
  @Output() favoriteToggle = new EventEmitter<Band>();

  toggleFav(event: Event): void {
    event.stopPropagation();
    this.favoriteToggle.emit(this.band);
  }
}
