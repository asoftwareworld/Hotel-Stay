import {
  Component, Input, Output, EventEmitter,
  ChangeDetectionStrategy, OnInit
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HotelRoom, DocumentType } from '../../../core/models/hotel-room.model';
import { ReserveRequest } from '../../../core/models/reservation.model';
import { ProviderBadgeComponent } from '../../../shared/components/provider-badge/provider-badge.component';
import { getDestinationClass } from '../../../core/constants/city-classifications';

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, ProviderBadgeComponent],
  templateUrl: './reservation-form.component.html',
  styleUrl: './reservation-form.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReservationFormComponent implements OnInit {
  @Input({ required: true }) room!: HotelRoom;
  @Input({ required: true }) destination!: string;
  @Input() checkIn: string = '';
  @Input() checkOut: string = '';

  @Output() reserve = new EventEmitter<ReserveRequest>();
  @Output() cancel = new EventEmitter<void>();

  form!: FormGroup;

  get isInternational(): boolean {
    return getDestinationClass(this.destination) === 'international';
  }

  get allowedDocumentTypes(): DocumentType[] {
    if (this.isInternational) {
      return ['Passport'];
    }
    return ['Passport', 'NationalId'];
  }

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    const defaultDocType = this.allowedDocumentTypes[0];
    this.form = this.fb.group({
      guestName: ['', [Validators.required, Validators.minLength(2)]],
      documentType: [defaultDocType, Validators.required],
      documentNumber: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  get guestName() { return this.form.get('guestName')!; }
  get documentType() { return this.form.get('documentType')!; }
  get documentNumber() { return this.form.get('documentNumber')!; }

  get guestNameInvalid(): boolean {
    return this.guestName.invalid && this.guestName.touched;
  }

  get documentNumberInvalid(): boolean {
    return this.documentNumber.invalid && this.documentNumber.touched;
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

  getDocTypeLabel(docType: string): string {
    return docType === 'NationalId' ? 'National ID' : docType;
  }

  onSubmit(): void {
    if (this.form.valid) {
      const values = this.form.value as {
        guestName: string;
        documentType: DocumentType;
        documentNumber: string;
      };
      const request: ReserveRequest = {
        provider: this.room.provider,
        roomType: this.room.roomType,
        destination: this.destination,
        checkIn: this.checkIn,
        checkOut: this.checkOut,
        perNightRate: this.room.perNightRate,
        cancellationPolicy: this.room.cancellationPolicy,
        guestName: values.guestName,
        documentType: values.documentType,
        documentNumber: values.documentNumber
      };
      this.reserve.emit(request);
    } else {
      this.form.markAllAsTouched();
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
