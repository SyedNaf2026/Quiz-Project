import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ToastService {

  private show(message: string, type: 'success' | 'error' | 'info'): void {
    const bg = type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#3b82f6';
    const icon = type === 'success' ? '✅' : type === 'error' ? '❌' : 'ℹ️';
    const el = document.createElement('div');
    el.style.cssText = `position:fixed;top:1rem;right:1rem;z-index:99999;background:${bg};color:#fff;padding:0.875rem 1.25rem;border-radius:8px;font-size:0.9rem;font-weight:500;box-shadow:0 4px 12px rgba(0,0,0,0.3);display:flex;align-items:center;gap:0.5rem;min-width:260px;cursor:pointer;font-family:Inter,sans-serif;`;
    el.innerHTML = `<span>${icon}</span><span>${message}</span>`;
    el.onclick = () => el.remove();
    document.body.appendChild(el);
    setTimeout(() => { if (el.parentNode) el.remove(); }, 3000);
  }

  success(msg: string) { this.show(msg, 'success'); }
  error(msg: string) { this.show(msg, 'error'); }
  info(msg: string) { this.show(msg, 'info'); }
}
