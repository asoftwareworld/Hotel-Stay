import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HotelService } from '../../../core/services/hotel.service';
import { HotelRoom, SearchQuery } from '../../../core/models/hotel-room.model';
import { ReservationDetail, ReserveRequest } from '../../../core/models/reservation.model';
import { SearchFormComponent } from '../search-form/search-form.component';
import { ResultsListComponent } from '../results-list/results-list.component';
import { ReservationFormComponent } from '../../reservation/reservation-form/reservation-form.component';
import { ConfirmationComponent } from '../../reservation/confirmation/confirmation.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ErrorBannerComponent } from '../../../shared/components/error-banner/error-banner.component';

type ViewState = 'search' | 'results' | 'reserve' | 'confirm';

@Component({
  selector: 'app-search-page',
  standalone: true,
  imports: [
    CommonModule,
    SearchFormComponent,
    ResultsListComponent,
    ReservationFormComponent,
    ConfirmationComponent,
    LoadingSpinnerComponent,
    ErrorBannerComponent
  ],
  templateUrl: './search-page.component.html',
  styleUrl: './search-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchPageComponent {
  private readonly hotelService = inject(HotelService);

  searchResults = signal<HotelRoom[]>([]);
  selectedRoom = signal<HotelRoom | null>(null);
  reservation = signal<ReservationDetail | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
  view = signal<ViewState>('search');
  lastDestination = signal<string>('');
  lastCheckIn = signal<string>('');
  lastCheckOut = signal<string>('');

  onSearch(query: SearchQuery): void {
    this.isLoading.set(true);
    this.error.set(null);
    this.searchResults.set([]);
    this.lastDestination.set(query.destination);
    this.lastCheckIn.set(query.checkIn);
    this.lastCheckOut.set(query.checkOut);

    this.hotelService.searchRooms(query.destination, query.checkIn, query.checkOut, query.roomType).subscribe({
      next: (result) => {
        this.searchResults.set(result.results);
        this.view.set('results');
        this.isLoading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.isLoading.set(false);
      }
    });
  }

  onSelectRoom(room: HotelRoom): void {
    this.selectedRoom.set(room);
    this.view.set('reserve');
  }

  onReserve(request: ReserveRequest): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.hotelService.reserve(request).subscribe({
      next: (detail) => {
        this.reservation.set(detail);
        this.view.set('confirm');
        this.isLoading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message);
        this.isLoading.set(false);
      }
    });
  }

  onDismissError(): void {
    this.error.set(null);
  }

  onNewSearch(): void {
    this.searchResults.set([]);
    this.selectedRoom.set(null);
    this.reservation.set(null);
    this.isLoading.set(false);
    this.error.set(null);
    this.view.set('search');
    this.lastDestination.set('');
    this.lastCheckIn.set('');
    this.lastCheckOut.set('');
  }

  onCancelReservation(): void {
    this.view.set('results');
    this.selectedRoom.set(null);
  }
}
