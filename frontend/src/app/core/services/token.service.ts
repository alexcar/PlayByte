import { Injectable } from '@angular/core';

const TOKEN_KEY = 'pb.accessToken';
const EXPIRES_KEY = 'pb.expiresAtUtc';
const USER_ID_KEY = 'pb.userId';

/** Persistência do JWT em localStorage para sobreviver a recargas de página. */
@Injectable({ providedIn: 'root' })
export class TokenService {
  save(token: string, expiresAtUtc: string, userId: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(EXPIRES_KEY, expiresAtUtc);
    localStorage.setItem(USER_ID_KEY, userId);
  }

  get token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  get userId(): string | null {
    return localStorage.getItem(USER_ID_KEY);
  }

  /** Token presente e ainda dentro do prazo de validade. */
  get isValid(): boolean {
    const token = this.token;
    const expires = localStorage.getItem(EXPIRES_KEY);
    if (!token || !expires) return false;
    return new Date(expires).getTime() > Date.now();
  }

  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXPIRES_KEY);
    localStorage.removeItem(USER_ID_KEY);
  }
}
