import { Component, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../service/auth.service';
import { ToastService } from '../service/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  form: FormGroup;
  loading = false;
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      fullName: ['', [
        Validators.required,
        Validators.minLength(2),
        Validators.pattern(/^\S+$/)  // no spaces allowed
      ]],
      email: ['', [
        Validators.required,
        Validators.pattern(/^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/)  // must have valid domain
      ]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['QuizTaker', Validators.required]
    });
  }

  get f() { return this.form.controls; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.cdr.detectChanges(); // prevent NG0100 ExpressionChangedAfterChecked
    this.auth.register(this.form.value).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.auth.saveSession(res.data);
          this.toast.success('Account created successfully!');
          this.router.navigate(['/home']);
        } else {
          this.toast.error(res.message);
        }
      },
      error: (err) => {
        this.loading = false;
        this.toast.error(err?.error?.message || 'Registration failed. Please try again.');
      }
    });
  }
}
