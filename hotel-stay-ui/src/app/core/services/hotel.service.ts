import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SearchResult } from '../models/hotel-room.model';
import { ReserveRequest, ReservationDetail } from '../models/reservation.model';

@Injectable({ providedIn: 'root' })
export class HotelService {
  private readonly http = inject(HttpClient);

  searchRooms(destination: string, checkIn: string, checkOut: string, roomType?: string): Observable<SearchResult> {
    let params = new HttpParams()
      .set('destination', destination)
      .set('checkIn', checkIn)
      .set('checkOut', checkOut);
    if (roomType) {
      params = params.set('roomType', roomType);
    }
    return this.http.get<SearchResult>('/hotels/search', { params });
  }

  reserve(request: ReserveRequest): Observable<ReservationDetail> {
    return this.http.post<ReservationDetail>('/hotels/reserve', request);
  }

  getReservation(reference: string): Observable<ReservationDetail> {
    return this.http.get<ReservationDetail>(`/hotels/reservation/${reference}`);
  }
}
