import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-provider-badge',
  standalone: true,
  imports: [NgClass],
  templateUrl: './provider-badge.component.html',
  styleUrl: './provider-badge.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProviderBadgeComponent {
  @Input({ required: true }) provider!: string;

  get badgeClass(): 'badge-premier' | 'badge-budget' {
    const lower = this.provider.toLowerCase();
    if (lower.includes('premier') || lower.includes('luxury') || lower.includes('grand')) {
      return 'badge-premier';
    }
    return 'badge-budget';
  }
}
