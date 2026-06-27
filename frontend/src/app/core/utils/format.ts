/**
 * Helpers de apresentação. A API não fornece campos puramente visuais (emoji do
 * catálogo, duração formatada, avatar), então derivamos aqui de forma determinística
 * para manter a UI consistente entre renderizações.
 */

const MUSIC_EMOJIS = ['🎸', '🎹', '🎷', '🥁', '🎺', '🎻', '🎤', '🎧', '🎵', '🎶'];

/** Hash simples e estável a partir de uma string (ex.: id/guid). */
function hash(seed: string): number {
  let h = 0;
  for (let i = 0; i < seed.length; i++) {
    h = (h << 5) - h + seed.charCodeAt(i);
    h |= 0;
  }
  return Math.abs(h);
}

/** Emoji estável escolhido a partir de um seed (id ou nome). */
export function emojiFor(seed: string): string {
  return MUSIC_EMOJIS[hash(seed) % MUSIC_EMOJIS.length];
}

/** Converte duração em segundos (API) para "m:ss" (UI). */
export function formatDuration(totalSeconds: number): string {
  if (!Number.isFinite(totalSeconds) || totalSeconds < 0) return '0:00';
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = Math.floor(totalSeconds % 60);
  return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}

/** "AAAA-MM-DD" (ou ISO) → "mês AAAA" em pt-BR (ex.: "jan 2025"). */
export function formatMonthYear(iso: string | undefined | null): string {
  if (!iso) return '—';
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleDateString('pt-BR', { month: 'short', year: 'numeric' });
}

/** "AAAA-MM-DD" (ou ISO) → "DD/MM/AAAA" em pt-BR. */
export function formatDateBr(iso: string | undefined | null): string {
  if (!iso) return '—';
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleDateString('pt-BR');
}

/** URL de avatar gerada a partir do nome (a API não armazena avatar). */
export function avatarUrlFor(name: string): string {
  const safe = encodeURIComponent(name || 'PlayByte');
  return `https://ui-avatars.com/api/?name=${safe}&background=1E2F40&color=E8F0F8&size=160`;
}
