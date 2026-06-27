import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // A raiz redireciona para a landing. Precisa vir antes das rotas de layout
  // com path '' (senão o auth-layout casa primeiro e mostra só o logo).
  { path: '', redirectTo: 'inicio', pathMatch: 'full' },

  // Páginas de autenticação (layout próprio, sem navbar interna)
  {
    path: '',
    loadComponent: () =>
      import('./layout/auth-layout/auth-layout.component').then((m) => m.AuthLayoutComponent),
    children: [
      {
        path: 'login',
        loadComponent: () =>
          import('./features/auth/login/login.component').then((m) => m.LoginComponent),
      },
      {
        path: 'cadastro',
        loadComponent: () =>
          import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
      },
      {
        path: 'esqueci-senha',
        loadComponent: () =>
          import('./features/auth/forgot-password/forgot-password.component').then(
            (m) => m.ForgotPasswordComponent,
          ),
      },
      {
        path: 'redefinir-senha',
        loadComponent: () =>
          import('./features/auth/reset-password/reset-password.component').then(
            (m) => m.ResetPasswordComponent,
          ),
      },
    ],
  },

  // Landing page e planos (públicas, com navbar pública)
  {
    path: '',
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then((m) => m.MainLayoutComponent),
    children: [
      {
        path: 'inicio',
        loadComponent: () =>
          import('./features/home/home.component').then((m) => m.HomeComponent),
      },
      {
        path: 'planos',
        loadComponent: () =>
          import('./features/plans/plans.component').then((m) => m.PlansComponent),
      },
      {
        path: 'catalogo',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/catalog/catalog.component').then((m) => m.CatalogComponent),
      },
      {
        path: 'player',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/player/player.component').then((m) => m.PlayerComponent),
      },
      {
        path: 'playlists',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/playlists/playlists.component').then((m) => m.PlaylistsComponent),
      },
      {
        path: 'favoritos',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/favorites/favorites.component').then((m) => m.FavoritesComponent),
      },
      {
        path: 'busca',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/search/search.component').then((m) => m.SearchComponent),
      },
      {
        path: 'perfil',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/profile/profile.component').then((m) => m.ProfileComponent),
      },
    ],
  },

  { path: '**', redirectTo: 'inicio' },
];
