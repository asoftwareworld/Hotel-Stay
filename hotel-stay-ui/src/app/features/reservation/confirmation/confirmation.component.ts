import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReservationDetail } from '../../../core/models/reservation.model';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirmation.component.html',
  styleUrl: './confirmation.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmationComponent {
  @Input({ required: true }) reservation!: ReservationDetail;
  @Output() newSearch = new EventEmitter<void>();

  get policyLabel(): string {
    const map: Record<string, string> = {
      FreeCancellation: 'Free Cancellation (up to 48h before check-in)',
      Flexible: 'Flexible (cancel up to 24h before check-in)',
      NonRefundable: 'Non-Refundable'
    };
    return map[this.reservation.cancellationPolicy] ?? this.reservation.cancellationPolicy;
  }

  get docTypeLabel(): string {
    return this.reservation.documentType === 'NationalId' ? 'National ID' : this.reservation.documentType;
  }

  onNewSearch(): void {
    this.newSearch.emit();
  }
}
