import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Navbar } from '../navbar/navbar';
import { UserService } from '../service/user.service';
import { ToastService } from '../service/toast.service';
import { UserStatsDTO } from '../models/models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Navbar],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile implements OnInit {
  form: FormGroup;
  loading = true;
  saving = false;
  role = '';
  stats: UserStatsDTO | null = null;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
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
      next: (res) => {
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
      next: (res) => { this.stats = res.data; this.cdr.detectChanges(); }
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    this.cdr.detectChanges();
    this.userService.updateProfile(this.form.value).subscribe({
      next: (res) => {
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
}
