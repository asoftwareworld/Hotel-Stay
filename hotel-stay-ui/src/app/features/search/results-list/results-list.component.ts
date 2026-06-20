import {
  Component, Input, Output, EventEmitter,
  signal, computed, WritableSignal, ChangeDetectionStrategy, OnChanges, SimpleChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { HotelRoom } from '../../../core/models/hotel-room.model';
import { ProviderBadgeComponent } from '../../../shared/components/provider-badge/provider-badge.component';

type SortOrder = 'asc' | 'desc';

@Component({
  selector: 'app-results-list',
  standalone: true,
  imports: [CommonModule, ProviderBadgeComponent],
  templateUrl: './results-list.component.html',
  styleUrl: './results-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResultsListComponent implements OnChanges {
  @Input({ required: true }) rooms!: HotelRoom[];
  @Output() selectRoom = new EventEmitter<HotelRoom>();

  private readonly _rooms: WritableSignal<HotelRoom[]> = signal([]);
  readonly sortOrder: WritableSignal<SortOrder> = signal('asc');

  readonly sortedRooms = computed(() => {
    const order = this.sortOrder();
    return [...this._rooms()].sort((a, b) =>
      order === 'asc' ? a.totalPrice - b.totalPrice : b.totalPrice - a.totalPrice
    );
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['rooms']) {
      this._rooms.set(this.rooms ?? []);
    }
  }

  toggleSort(): void {
    this.sortOrder.update(o => o === 'asc' ? 'desc' : 'asc');
  }

  onSelect(room: HotelRoom): void {
    this.selectRoom.emit(room);
  }

  getStars(n: number | null): string {
    if (!n) return '';
    return '★'.repeat(Math.min(Math.max(Math.round(n), 1), 5));
  }

  getPolicyBadgeClass(policy: string): string {
    switch (policy) {
      case 'FreeCancellation': return 'badge badge-policy-free';
      case 'Flexible': return 'badge badge-policy-flexible';
      case 'NonRefundable': return 'badge badge-policy-nonrefundable';
      default: return 'badge';
    }
  }

  getPolicyLabel(policy: string): string {
    switch (policy) {
      case 'FreeCancellation': return 'Free Cancellation';
      case 'Flexible': return 'Flexible';
      case 'NonRefundable': return 'Non-Refundable';
      default: return policy;
    }
  }

  getRoomTypeLabel(roomType: string): string {
    return roomType + ' Room';
  }
}
