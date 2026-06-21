import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  template: `
    @if (auth.isAuthenticated()) {
      <nav class="app-nav">
        <a routerLink="/search" class="nav-brand">HotelStay</a>
        <button class="btn-logout" (click)="auth.logout()">Sign out</button>
      </nav>
    }
    <router-outlet />
  `,
  styles: [`
    .app-nav {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem 2rem;
      background: #1a1a2e;
      color: #fff;
    }
    .nav-brand {
      color: #fff;
      font-weight: 700;
      font-size: 1.1rem;
      text-decoration: none;
      letter-spacing: 0.03em;
    }
    .btn-logout {
      background: transparent;
      border: 1px solid rgba(255,255,255,0.4);
      color: #fff;
      padding: 0.35rem 0.9rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.85rem;
      transition: background 0.15s;
    }
    .btn-logout:hover { background: rgba(255,255,255,0.1); }
  `],
})
export class AppComponent {
  readonly auth = inject(AuthService);
}
