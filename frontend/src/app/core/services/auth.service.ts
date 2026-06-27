import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, forkJoin, map, switchMap, tap, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User, PlanType } from '../models';
import {
  LoginResponseDto,
  UserResponseDto,
  MySubscriptionResponseDto,
} from '../api/dtos';
import { TokenService } from './token.service';
import { avatarUrlFor } from '../utils/format';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokens = inject(TokenService);
  private readonly api = environment.apiBaseUrl;

  private readonly _user = signal<User | null>(null);

  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);
  readonly plan = computed<PlanType>(() => this._user()?.plan ?? 'free');
  readonly isPaid = computed(() => this.plan() === 'paid');

  /** Reidrata a sessão a partir do token persistido (chamado no bootstrap). */
  restoreSession(): Observable<User | null> {
    const userId = this.tokens.userId;
    if (!this.tokens.isValid || !userId) {
      this.tokens.clear();
      return of(null);
    }
    return this.loadSession(userId).pipe(catchError(() => of(null)));
  }

  login(email: string, password: string): Observable<User> {
    return this.http
      .post<LoginResponseDto>(`${this.api}/auth/login`, { email, password })
      .pipe(
        tap((res) => this.tokens.save(res.accessToken, res.expiresAtUtc, res.userId)),
        switchMap((res) => this.loadSession(res.userId)),
      );
  }

  /** Cria a conta (US-01) e já autentica para obter o token JWT. */
  register(name: string, email: string, password: string): Observable<User> {
    return this.http
      .post(`${this.api}/users`, { name, email, password })
      .pipe(switchMap(() => this.login(email, password)));
  }

  logout(): void {
    this.tokens.clear();
    this._user.set(null);
  }

  /** Não revela se o e-mail existe na base (proteção contra enumeração — US-03 c3). */
  requestPasswordReset(email: string): Observable<void> {
    return this.http
      .post(`${this.api}/auth/forgot-password`, { email })
      .pipe(map(() => void 0));
  }

  resetPassword(token: string, newPassword: string): Observable<void> {
    return this.http
      .post(`${this.api}/auth/reset-password`, { token, newPassword })
      .pipe(map(() => void 0));
  }

  /** Atualiza nome e e-mail do perfil (PUT /users/me) e o usuário em sessão. */
  updateProfile(name: string, email: string): Observable<User> {
    return this.http
      .put<UserResponseDto>(`${this.api}/users/me`, { name, email })
      .pipe(
        map((res) => {
          const current = this._user();
          const updated: User = {
            id: res.id,
            name: res.name,
            email: res.email,
            avatarUrl: avatarUrlFor(res.name),
            plan: current?.plan ?? 'free',
            memberSince: res.createdAtUtc.substring(0, 10),
          };
          this._user.set(updated);
          return updated;
        }),
      );
  }

  /** Altera a senha (PUT /users/me/password); exige a senha atual. */
  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.http
      .put(`${this.api}/users/me/password`, { currentPassword, newPassword })
      .pipe(map(() => void 0));
  }

  /** Reconsulta a assinatura e atualiza o plano do usuário em sessão (após assinar/cancelar). */
  refreshSubscription(): Observable<User | null> {
    const current = this._user();
    if (!current) return of(null);
    return this.fetchPlan().pipe(
      map((plan) => {
        const updated: User = { ...current, plan };
        this._user.set(updated);
        return updated;
      }),
    );
  }

  /** Carrega usuário + plano e popula o signal de sessão. */
  private loadSession(userId: string): Observable<User> {
    return forkJoin({
      user: this.http.get<UserResponseDto>(`${this.api}/users/${userId}`),
      plan: this.fetchPlan(),
    }).pipe(
      map(({ user, plan }) => {
        const model: User = {
          id: user.id,
          name: user.name,
          email: user.email,
          avatarUrl: avatarUrlFor(user.name),
          plan,
          memberSince: user.createdAtUtc.substring(0, 10),
        };
        this._user.set(model);
        return model;
      }),
    );
  }

  /** Plano derivado da assinatura: acesso ativo → "paid"; ausência/404 → "free". */
  private fetchPlan(): Observable<PlanType> {
    return this.http
      .get<MySubscriptionResponseDto>(`${this.api}/subscriptions/me`)
      .pipe(
        map((sub): PlanType => (sub.hasActiveAccess ? 'paid' : 'free')),
        catchError(() => of<PlanType>('free')),
      );
  }
}
