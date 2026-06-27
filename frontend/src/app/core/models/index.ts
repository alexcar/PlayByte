export type PlanType = 'free' | 'paid';

export interface User {
  id: string;
  name: string;
  email: string;
  avatarUrl: string;
  plan: PlanType;
  memberSince: string;
}

export interface Band {
  id: string;
  name: string;
  genre: string;
  emoji: string;
  albumCount: number;
  coverImageUrl?: string | null;
  description?: string;
  favorite?: boolean;
}

export interface Album {
  id: string;
  bandId: string;
  title: string;
  year: number;
  emoji: string;
  tracks: Track[];
}

export interface Track {
  id: string;
  title: string;
  bandName: string;
  albumTitle?: string;
  duration: string;
  emoji: string;
  favorite?: boolean;
  streamUrl?: string;
}

export interface Playlist {
  id: string;
  name: string;
  emoji: string;
  trackCount: number;
  trackIds?: string[];
  tracks?: Track[];
}

export interface Plan {
  id: PlanType;
  name: string;
  priceMonthly: number;
  priceYearly: number;
  description: string;
  featured: boolean;
  features: PlanFeature[];
}

export interface PlanFeature {
  label: string;
  included: boolean;
  highlight?: boolean;
}

export interface SubscriptionEvent {
  id: string;
  type: 'created' | 'charge' | 'renewal' | 'cancel';
  title: string;
  description: string;
  date: string;
}
