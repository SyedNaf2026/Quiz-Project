import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (visible) {
      <div class="modal-backdrop">
        <div class="confirm-dialog">
          <div class="confirm-icon">⚠️</div>
          <div class="confirm-title">{{ title }}</div>
          <div class="confirm-message">{{ message }}</div>
          <div class="confirm-actions">
            <button class="btn btn-secondary" (click)="cancel.emit()">Cancel</button>
            <button class="btn btn-danger" (click)="confirm.emit()">{{ confirmLabel }}</button>
          </div>
        </div>
      </div>
    }
  `
})
export class ConfirmDialogComponent {
  @Input() visible = false;
  @Input() title = 'Confirm Action';
  @Input() message = 'Are you sure you want to proceed?';
  @Input() confirmLabel = 'Confirm';
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
}
