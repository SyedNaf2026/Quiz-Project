import { Component, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../service/auth.service';
import { ToastService } from '../service/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  form: FormGroup;
  loading = false;
  showPassword = false;
  showForgot = false;
  forgotEmail = '';
  forgotNewPassword = '';
  forgotConfirm = '';
  forgotLoading = false;
  forgotError = '';

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  get f() { return this.form.controls; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.auth.login(this.form.value).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.auth.saveSession(res.data);
          this.toast.success('Welcome back, ' + res.data.fullName + '!');
          this.router.navigate(['/home']);
        } else {
          this.toast.error(res.message || 'Invalid credentials.');
        }
      },
      error: (err) => {
        this.loading = false;
        this.toast.error(err?.error?.message || 'Invalid email or password.');
      }
    });
  }

  resetForgot(): void {
    this.forgotEmail = '';
    this.forgotNewPassword = '';
    this.forgotConfirm = '';
    this.forgotError = '';
    this.forgotLoading = false;
  }

  submitForgot(): void {
    this.forgotError = '';
    if (!this.forgotEmail.trim()) { this.forgotError = 'Email is required.'; return; }
    if (!this.forgotNewPassword || this.forgotNewPassword.length < 6) { this.forgotError = 'Password must be at least 6 characters.'; return; }
    if (this.forgotNewPassword !== this.forgotConfirm) { this.forgotError = 'Passwords do not match.'; return; }
    this.forgotLoading = true;
    this.auth.resetPassword(this.forgotEmail.trim(), this.forgotNewPassword).subscribe({
      next: (res) => {
        this.forgotLoading = false;
        if (res.success) {
          this.toast.success('Password reset! Please log in.');
          this.showForgot = false;
          this.resetForgot();
        } else {
          this.forgotError = res.message || 'Reset failed.';
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        this.forgotLoading = false;
        this.forgotError = err?.error?.message || 'Email not found.';
        this.cdr.detectChanges();
      }
    });
  }
}