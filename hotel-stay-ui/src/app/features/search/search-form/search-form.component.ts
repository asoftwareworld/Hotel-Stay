import { Component, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SearchQuery } from '../../../core/models/hotel-room.model';
import { ALL_CITIES } from '../../../core/constants/city-classifications';

function checkOutAfterCheckIn(group: AbstractControl): ValidationErrors | null {
  const checkIn = group.get('checkIn')?.value as string;
  const checkOut = group.get('checkOut')?.value as string;
  if (checkIn && checkOut && checkOut <= checkIn) {
    return { checkOutBeforeCheckIn: true };
  }
  return null;
}

@Component({
  selector: 'app-search-form',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './search-form.component.html',
  styleUrl: './search-form.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchFormComponent {
  @Output() search = new EventEmitter<SearchQuery>();

  readonly allCities = ALL_CITIES;

  readonly form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group(
      {
        destination: ['', Validators.required],
        checkIn: ['', Validators.required],
        checkOut: ['', Validators.required],
        roomType: ['']
      },
      { validators: checkOutAfterCheckIn }
    );
  }

  get destination() { return this.form.get('destination')!; }
  get checkIn() { return this.form.get('checkIn')!; }
  get checkOut() { return this.form.get('checkOut')!; }
  get roomType() { return this.form.get('roomType')!; }

  get destinationInvalid(): boolean {
    return this.destination.invalid && this.destination.touched;
  }

  get checkInInvalid(): boolean {
    return this.checkIn.invalid && this.checkIn.touched;
  }

  get checkOutInvalid(): boolean {
    return this.checkOut.invalid && this.checkOut.touched;
  }

  get checkOutBeforeCheckIn(): boolean {
    return !!this.form.errors?.['checkOutBeforeCheckIn'] && this.checkOut.touched;
  }

  get today(): string {
    return new Date().toISOString().split('T')[0];
  }

  onSubmit(): void {
    if (this.form.valid) {
      const { destination, checkIn, checkOut, roomType } = this.form.value as {
        destination: string;
        checkIn: string;
        checkOut: string;
        roomType: string;
      };
      const query: SearchQuery = { destination, checkIn, checkOut };
      if (roomType) query.roomType = roomType;
      this.search.emit(query);
    } else {
      this.form.markAllAsTouched();
    }
  }
}
