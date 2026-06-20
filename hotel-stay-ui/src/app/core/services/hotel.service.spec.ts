import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HotelService } from './hotel.service';
import { SearchResult } from '../models/hotel-room.model';
import { ReservationDetail, ReserveRequest } from '../models/reservation.model';

describe('HotelService', () => {
  let service: HotelService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [HotelService]
    });
    service = TestBed.inject(HotelService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('searchRooms', () => {
    it('should build correct URL with required params', () => {
      const mockResult: SearchResult = {
        results: [],
        destination: 'Oslo',
        checkIn: '2024-06-01',
        checkOut: '2024-06-05',
        totalResults: 0
      };

      service.searchRooms('Oslo', '2024-06-01', '2024-06-05').subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne(r =>
        r.url === '/hotels/search' &&
        r.params.get('destination') === 'Oslo' &&
        r.params.get('checkIn') === '2024-06-01' &&
        r.params.get('checkOut') === '2024-06-05'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResult);
    });

    it('should include roomType param when provided', () => {
      const mockResult: SearchResult = {
        results: [],
        destination: 'Oslo',
        checkIn: '2024-06-01',
        checkOut: '2024-06-05',
        totalResults: 0
      };

      service.searchRooms('Oslo', '2024-06-01', '2024-06-05', 'Deluxe').subscribe(result => {
        expect(result).toEqual(mockResult);
      });

      const req = httpMock.expectOne(r =>
        r.url === '/hotels/search' &&
        r.params.get('destination') === 'Oslo' &&
        r.params.get('checkIn') === '2024-06-01' &&
        r.params.get('checkOut') === '2024-06-05' &&
        r.params.get('roomType') === 'Deluxe'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResult);
    });

    it('should not include roomType param when not provided', () => {
      const mockResult: SearchResult = {
        results: [],
        destination: 'Bergen',
        checkIn: '2024-07-10',
        checkOut: '2024-07-14',
        totalResults: 0
      };

      service.searchRooms('Bergen', '2024-07-10', '2024-07-14').subscribe();

      const req = httpMock.expectOne(r =>
        r.url === '/hotels/search'
      );
      expect(req.request.params.has('roomType')).toBeFalse();
      req.flush(mockResult);
    });
  });

  describe('reserve', () => {
    it('should POST to /hotels/reserve with the provided request body', () => {
      const reserveRequest: ReserveRequest = {
        provider: 'Premier Inn',
        roomType: 'Deluxe',
        destination: 'Oslo',
        checkIn: '2024-06-01',
        checkOut: '2024-06-05',
        perNightRate: 149,
        cancellationPolicy: 'FreeCancellation',
        guestName: 'John Doe',
        documentType: 'Passport',
        documentNumber: 'AB123456'
      };

      const mockDetail: ReservationDetail = {
        reference: 'REF-001',
        provider: 'Premier Inn',
        roomType: 'Deluxe',
        destination: 'Oslo',
        checkIn: '2024-06-01',
        checkOut: '2024-06-05',
        nights: 4,
        perNightRate: 149,
        totalPrice: 596,
        cancellationPolicy: 'FreeCancellation',
        guestName: 'John Doe',
        documentType: 'Passport'
      };

      service.reserve(reserveRequest).subscribe(detail => {
        expect(detail).toEqual(mockDetail);
      });

      const req = httpMock.expectOne('/hotels/reserve');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(reserveRequest);
      req.flush(mockDetail);
    });
  });

  describe('getReservation', () => {
    it('should GET reservation by reference', () => {
      const mockDetail: ReservationDetail = {
        reference: 'REF-001',
        provider: 'Budget Stay',
        roomType: 'Standard',
        destination: 'Bergen',
        checkIn: '2024-08-01',
        checkOut: '2024-08-03',
        nights: 2,
        perNightRate: 89,
        totalPrice: 178,
        cancellationPolicy: 'NonRefundable',
        guestName: 'Jane Smith',
        documentType: 'NationalId'
      };

      service.getReservation('REF-001').subscribe(detail => {
        expect(detail).toEqual(mockDetail);
      });

      const req = httpMock.expectOne('/hotels/reservation/REF-001');
      expect(req.request.method).toBe('GET');
      req.flush(mockDetail);
    });
  });
});
