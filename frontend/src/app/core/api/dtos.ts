/**
 * DTOs que espelham os contratos da API C# PlayByte (camelCase via System.Text.Json).
 * Mantidos separados dos modelos de UI (../models): os serviços fazem a tradução.
 */

// ---- Autenticação / Usuário ----
export interface LoginResponseDto {
  userId: string;
  accessToken: string;
  expiresAtUtc: string;
}

export interface UserResponseDto {
  id: string;
  name: string;
  email: string;
  isActive: boolean;
  createdAtUtc: string;
}

// ---- Catálogo ----
export interface PagedResultDto<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
}

export interface BandListItemDto {
  id: string;
  name: string;
  coverImageUrl: string | null;
}

export interface BandDetailsDto {
  id: string;
  name: string;
  coverImageUrl: string | null;
  albums: AlbumDto[];
}

export interface AlbumDto {
  id: string;
  title: string;
  releaseYear: number;
  tracks: TrackDto[];
}

export interface TrackDto {
  id: string;
  title: string;
  durationSeconds: number;
}

export interface PlaybackResponseDto {
  trackId: string;
  title: string;
  bandName: string;
  streamUrl: string;
}

// ---- Busca ----
export interface SearchResponseDto {
  bands: SearchBandItemDto[];
  tracks: SearchTrackItemDto[];
}
export interface SearchBandItemDto {
  id: string;
  name: string;
  coverImageUrl: string | null;
}
export interface SearchTrackItemDto {
  id: string;
  title: string;
  bandName: string;
}

// ---- Playlists ----
export interface PlaylistSummaryDto {
  id: string;
  name: string;
  trackCount: number;
}
export interface PlaylistDetailsDto {
  id: string;
  name: string;
  trackCount: number;
  tracks: PlaylistTrackDto[];
}
export interface PlaylistTrackDto {
  trackId: string;
  title: string;
  bandName: string;
  position: number;
}

// ---- Favoritos ----
export interface FavoriteTrackItemDto {
  trackId: string;
  title: string;
  bandName: string;
}
export interface FavoriteBandItemDto {
  bandId: string;
  name: string;
  coverImageUrl: string | null;
}

// ---- Assinatura ----
export interface SubscribeResponseDto {
  subscriptionId: string;
  paymentId: string;
}
export interface MySubscriptionResponseDto {
  subscriptionId: string;
  planName: string;
  status: string;
  currentPeriodEnd: string;
  hasActiveAccess: boolean;
}
