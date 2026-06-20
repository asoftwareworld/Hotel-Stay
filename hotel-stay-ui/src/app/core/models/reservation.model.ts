import { CancellationPolicy, DocumentType, RoomType } from './hotel-room.model';

export interface ReserveRequest {
  provider: string;
  roomType: RoomType;
  destination: string;
  checkIn: string;
  checkOut: string;
  perNightRate: number;
  cancellationPolicy: CancellationPolicy;
  guestName: string;
  documentType: DocumentType;
  documentNumber: string;
}

export interface ReservationDetail {
  reference: string;
  provider: string;
  roomType: RoomType;
  destination: string;
  checkIn: string;
  checkOut: string;
  nights: number;
  perNightRate: number;
  totalPrice: number;
  cancellationPolicy: CancellationPolicy;
  guestName: string;
  documentType: DocumentType;
}
