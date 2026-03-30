import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Navbar } from '../../navbar/navbar';
import { GroupService } from '../../service/group.service';
import { ToastService } from '../../service/toast.service';
import { GroupDTO } from '../../models/models';

@Component({
  selector: 'app-gm-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, Navbar],
  templateUrl: './gm-dashboard.html'
})
export class GmDashboard implements OnInit {
  groups: GroupDTO[] = [];
  loading = true;
  showForm = false;
  form: FormGroup;

  constructor(
    private groupService: GroupService,
    private toast: ToastService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: ['']
    });
  }

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.groupService.getMyGroups().subscribe({
      next: res => {
        this.groups = res.data || [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
        this.toast.error('Failed to load groups.');
      }
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.groupService.createGroup(this.form.value).subscribe({
      next: res => {
        if (res.success) {
          this.toast.success('Group created!');
          this.showForm = false;
          this.form.reset();
          this.load();
        } else {
          this.toast.error(res.message);
        }
      },
      error: () => this.toast.error('Failed to create group.')
    });
  }

  deleteGroup(id: number): void {
    if (!confirm('Delete this group?')) return;
    this.groupService.deleteGroup(id).subscribe({
      next: () => { this.toast.success('Group deleted.'); this.load(); },
      error: () => this.toast.error('Failed to delete group.')
    });
  }
}
