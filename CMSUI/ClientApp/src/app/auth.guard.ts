import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(): boolean {
    try {
      const raw = sessionStorage.getItem('usuario');
      if (raw) {
        const u = JSON.parse(raw);
        if (u?.token) return true;
      }
    } catch {}
    this.router.navigate(['/login']);
    return false;
  }
}
