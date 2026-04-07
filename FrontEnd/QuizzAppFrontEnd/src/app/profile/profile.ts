import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Navbar } from '../navbar/navbar';
import { UserService } from '../service/user.service';
import { AuthService } from '../service/auth.service';
import { ToastService } from '../service/toast.service';
import { UserStatsDTO } from '../models/models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, Navbar],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile implements OnInit {
  form: FormGroup;
  loading = true;
  saving = false;
  role = '';
  stats: UserStatsDTO | null = null;

  // Payment modal
  showPaymentModal = false;
  paymentLoading = false;
  cardNumber = '';
  cardExpiry = '';
  cardCvv = '';
  cardName = '';
  paymentError = '';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private auth: AuthService,
    private router: Router,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    this.userService.getProfile().subscribe({
      next: res => {
        if (res.data) {
          this.form.patchValue({ fullName: res.data.fullName, email: res.data.email });
          this.role = res.data.role;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });

    this.userService.getStats().subscribe({
      next: res => { this.stats = res.data; this.cdr.detectChanges(); }
    });
  }

  get isPremium(): boolean { return this.role === 'PremiumTaker'; }
  get isTaker(): boolean { return this.role === 'QuizTaker' || this.role === 'PremiumTaker'; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    this.cdr.detectChanges();
    this.userService.updateProfile(this.form.value).subscribe({
      next: res => {
        this.saving = false;
        this.cdr.detectChanges();
        if (res.success) {
          localStorage.setItem('user-name', this.form.value.fullName);
          localStorage.setItem('user-email', this.form.value.email);
          this.toast.success('Profile updated!');
        } else {
          this.toast.error(res.message);
        }
      },
      error: () => { this.saving = false; this.cdr.detectChanges(); this.toast.error('Update failed.'); }
    });
  }

  openPaymentModal(): void {
    this.paymentError = '';
    this.cardNumber = '';
    this.cardExpiry = '';
    this.cardCvv = '';
    this.cardName = '';
    this.showPaymentModal = true;
    this.cdr.detectChanges();
  }

  closePaymentModal(): void {
    this.showPaymentModal = false;
    this.cdr.detectChanges();
  }

  processPayment(): void {
    // Basic validation
    if (!this.cardName.trim()) { this.paymentError = 'Name on card is required.'; return; }
    if (this.cardNumber.replace(/\s/g, '').length < 16) { this.paymentError = 'Enter a valid 16-digit card number.'; return; }
    if (!this.cardExpiry.match(/^\d{2}\/\d{2}$/)) { this.paymentError = 'Enter expiry as MM/YY.'; return; }
    if (this.cardCvv.length < 3) { this.paymentError = 'Enter a valid CVV.'; return; }

    this.paymentError = '';
    this.paymentLoading = true;
    this.cdr.detectChanges();

    // Simulate payment processing delay
    setTimeout(() => {
      this.userService.upgradeToPremium().subscribe({
        next: res => {
          this.paymentLoading = false;
          if (res.success && res.data) {
            // Save new token and role
            localStorage.setItem('JWT-token', res.data.token);
            localStorage.setItem('user-role', res.data.role);
            this.role = res.data.role;
            this.showPaymentModal = false;
            this.toast.success('🎉 Welcome to Premium! Unlimited quiz attempts unlocked.');
            this.cdr.detectChanges();
            // Navigate away and back to force navbar to reload with new role
            this.router.navigateByUrl('/home').then(() => {
              this.router.navigate(['/profile']);
            });
          } else {
            this.paymentError = res.message || 'Upgrade failed.';
            this.paymentLoading = false;
            this.cdr.detectChanges();
          }
        },
        error: () => {
          this.paymentLoading = false;
          this.paymentError = 'Something went wrong. Please try again.';
          this.cdr.detectChanges();
        }
      });
    }, 2000); // 2 second simulated processing
  }

  formatCardNumber(event: Event): void {
    const input = event.target as HTMLInputElement;
    let val = input.value.replace(/\D/g, '').substring(0, 16);
    this.cardNumber = val.replace(/(.{4})/g, '$1 ').trim();
  }
}
