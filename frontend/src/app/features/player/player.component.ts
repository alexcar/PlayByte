import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SliderModule } from 'primeng/slider';
import { FormsModule } from '@angular/forms';
import { PlayerService } from '../../core/services/player.service';
import { AuthService } from '../../core/services/auth.service';
import { Track } from '../../core/models';

@Component({
  selector: 'pb-player',
  standalone: true,
  imports: [RouterLink, FormsModule, ButtonModule, SliderModule],
  templateUrl: './player.component.html',
  styleUrl: './player.component.scss',
})
export class PlayerComponent {
  protected readonly player = inject(PlayerService);
  protected readonly auth = inject(AuthService);

  protected volume = 70;
  protected readonly waveBars = Array.from({ length: 21 }, (_, i) => 6 + Math.round(Math.abs(Math.sin(i * 1.3)) * 24));

  protected readonly queue = signal<Track[]>([
    { id: 't1', title: 'Death on Two Legs', bandName: 'Queen', duration: '3:43', emoji: '🎸' },
    { id: 't2', title: 'Lazing on a Sunday Afternoon', bandName: 'Queen', duration: '1:08', emoji: '🎸' },
    { id: 't4', title: "You're My Best Friend", bandName: 'Queen', duration: '2:52', emoji: '🎸' },
    { id: 't17', title: "'39", bandName: 'Queen', duration: '3:31', emoji: '🎸' },
    { id: 't18', title: 'Love of My Life', bandName: 'Queen', duration: '3:38', emoji: '🎸' },
  ]);

  selectTrack(track: Track): void {
    this.player.play(track);
  }
}
