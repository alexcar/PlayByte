import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { of } from 'rxjs';
import { CatalogService } from '../../core/services/catalog.service';
import { AuthService } from '../../core/services/auth.service';
import { Band } from '../../core/models';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'pb-home',
  standalone: true,
  imports: [RouterLink, ButtonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  private readonly catalog = inject(CatalogService);
  protected readonly auth = inject(AuthService);
  // A landing é pública; o catálogo exige autenticação. Só busca bandas se logado.
  protected readonly bands = toSignal(
    this.auth.isAuthenticated() ? this.catalog.getBands() : of<Band[]>([]),
    { initialValue: [] as Band[] },
  );

  protected readonly waveBars = Array.from({ length: 21 }, (_, i) => 12 + Math.round(Math.abs(Math.sin(i)) * 32));

  protected readonly features = [
    { icon: 'pi-list', title: 'Catálogo completo', desc: 'Explore bandas, álbuns e faixas. Disponível gratuitamente para todos.' },
    { icon: 'pi-play-circle', title: 'Suas playlists', desc: 'Crie e organize playlists personalizadas com as músicas que você ama.' },
    { icon: 'pi-heart', title: 'Favoritos', desc: 'Salve músicas e bandas favoritas para acessar sempre que quiser.' },
    { icon: 'pi-volume-up', title: 'Reprodução ilimitada', desc: 'Ouça todas as músicas do catálogo sem limites com o plano pago.' },
  ];
}
