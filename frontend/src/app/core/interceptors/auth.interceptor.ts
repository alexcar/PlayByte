import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenService } from '../services/token.service';

/**
 * Anexa o JWT (Authorization: Bearer) às chamadas da API e, em 401, limpa a sessão
 * e redireciona para o login (US-02 c3).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokens = inject(TokenService);
  const router = inject(Router);

  const isApiCall = req.url.startsWith(environment.apiBaseUrl);
  const token = tokens.token;

  const authReq =
    isApiCall && token
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Só tratamos como sessão expirada quando havia token (token inválido/vencido).
      // Chamadas anônimas (ex.: landing page) recebem 401 sem disparar redirecionamento.
      if (error.status === 401 && isApiCall && token) {
        tokens.clear();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    }),
  );
};
