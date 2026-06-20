export type RoomType = 'Standard' | 'Deluxe' | 'Suite';
export type CancellationPolicy = 'FreeCancellation' | 'Flexible' | 'NonRefundable';
export type DocumentType = 'Passport' | 'NationalId';

export interface HotelRoom {
  provider: string;
  roomType: RoomType;
  perNightRate: number;
  totalPrice: number;
  nights: number;
  cancellationPolicy: CancellationPolicy;
  amenities: string[] | null;
  starRating: number | null;
}

export interface SearchResult {
  results: HotelRoom[];
  destination: string;
  checkIn: string;
  checkOut: string;
  totalResults: number;
}

export interface SearchQuery {
  destination: string;
  checkIn: string;
  checkOut: string;
  roomType?: string;
}
