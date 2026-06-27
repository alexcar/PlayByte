import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';

/** Protege rotas que exigem usuário autenticado. Redireciona ao login se não houver token válido. */
export const authGuard: CanActivateFn = (_route, state) => {
  const tokens = inject(TokenService);
  const router = inject(Router);

  if (tokens.isValid) return true;

  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url },
  });
};
