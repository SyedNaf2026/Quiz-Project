import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const router = inject(Router);
    const role = localStorage.getItem('user-role');
    if (role && allowedRoles.includes(role)) return true;
    router.navigate(['/home']);
    return false;
  };
};
