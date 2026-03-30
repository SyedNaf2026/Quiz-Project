import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Navbar } from '../../navbar/navbar';
import { GroupService } from '../../service/group.service';
import { QuizService } from '../../service/quiz.service';
import { CategoryService } from '../../service/category.service';
import { ToastService } from '../../service/toast.service';
import {
  GroupDTO, GroupMemberDTO, GroupQuizDTO,
  GroupQuizResultDTO, UserSearchDTO, QuizDTO, CategoryDTO
} from '../../models/models';

@Component({
  selector: 'app-group-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule, Navbar],
  templateUrl: './group-detail.html'
})
export class GroupDetail implements OnInit {
  groupId = 0;
  group: GroupDTO | null = null;
  members: GroupMemberDTO[] = [];
  groupQuizzes: GroupQuizDTO[] = [];
  submissions: GroupQuizResultDTO[] = [];
  allQuizzes: QuizDTO[] = [];
  categories: CategoryDTO[] = [];

  activeTab: 'members' | 'quizzes' | 'create-quiz' | 'submissions' = 'members';

  // Member search
  searchQuery = '';
  searchResults: UserSearchDTO[] = [];
  searchLoading = false;

  // Create quiz form
  quizForm: FormGroup;
  quizLoading = false;
  showNewCategory = false;
  newCategoryName = '';
  newCategoryDesc = '';

  loading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private groupService: GroupService,
    private quizService: QuizService,
    private categoryService: CategoryService,
    private toast: ToastService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.quizForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      categoryId: ['', Validators.required],
      timeLimit: [null],
      difficulty: ['Medium']
    });
  }

  ngOnInit(): void {
    this.groupId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadAll();
  }

  loadAll(): void {
    this.loading = true;
    this.groupService.getGroupById(this.groupId).subscribe({
      next: res => { this.group = res.data; this.loading = false; this.cdr.detectChanges(); },
      error: () => { this.loading = false; this.cdr.detectChanges(); this.toast.error('Failed to load group.'); }
    });
    this.loadMembers();
    this.loadGroupQuizzes();
    this.loadSubmissions();
    this.quizService.getActiveQuizzes().subscribe({
      next: res => { this.allQuizzes = res.data || []; this.cdr.detectChanges(); }
    });
    this.categoryService.getAllCategories().subscribe({
      next: res => { this.categories = res.data || []; this.cdr.detectChanges(); }
    });
  }

  loadMembers(): void {
    this.groupService.getMembers(this.groupId).subscribe({
      next: res => { this.members = res.data || []; this.cdr.detectChanges(); }
    });
  }

  loadGroupQuizzes(): void {
    this.groupService.getGroupQuizzes(this.groupId).subscribe({
      next: res => { this.groupQuizzes = res.data || []; this.cdr.detectChanges(); }
    });
  }

  loadSubmissions(): void {
    this.groupService.getGroupSubmissions(this.groupId).subscribe({
      next: res => { this.submissions = res.data || []; this.cdr.detectChanges(); }
    });
  }

  // Member search
  onSearch(): void {
    if (!this.searchQuery.trim()) { this.searchResults = []; return; }
    this.searchLoading = true;
    this.groupService.searchUsers(this.searchQuery).subscribe({
      next: res => { this.searchResults = res.data || []; this.searchLoading = false; this.cdr.detectChanges(); },
      error: () => { this.searchLoading = false; }
    });
  }

  addMember(user: UserSearchDTO): void {
    this.groupService.addMember(this.groupId, user.id).subscribe({
      next: res => {
        if (res.success) {
          this.toast.success(`${user.fullName} added.`);
          this.searchResults = this.searchResults.filter(u => u.id !== user.id);
          this.loadMembers();
        } else {
          this.toast.error(res.message);
        }
      },
      error: () => this.toast.error('Failed to add member.')
    });
  }

  removeMember(member: GroupMemberDTO): void {
    if (!confirm(`Remove ${member.fullName}?`)) return;
    this.groupService.removeMember(this.groupId, member.userId).subscribe({
      next: () => { this.toast.success('Member removed.'); this.loadMembers(); },
      error: () => this.toast.error('Failed to remove member.')
    });
  }

  // Quiz assignment
  isAssigned(quizId: number): boolean {
    return this.groupQuizzes.some(gq => gq.quizId === quizId);
  }

  assignQuiz(quizId: number): void {
    this.groupService.assignQuiz(this.groupId, quizId).subscribe({
      next: res => {
        if (res.success) { this.toast.success('Quiz assigned.'); this.loadGroupQuizzes(); }
        else this.toast.error(res.message);
      },
      error: () => this.toast.error('Failed to assign quiz.')
    });
  }

  removeQuiz(quizId: number): void {
    this.groupService.removeQuiz(this.groupId, quizId).subscribe({
      next: () => { this.toast.success('Quiz removed.'); this.loadGroupQuizzes(); },
      error: () => this.toast.error('Failed to remove quiz.')
    });
  }

  // Create quiz for this group
  addCategory(): void {
    if (!this.newCategoryName.trim()) { this.toast.error('Category name is required.'); return; }
    this.categoryService.createCategory({ name: this.newCategoryName.trim(), description: this.newCategoryDesc.trim() }).subscribe({
      next: res => {
        if (res.success && res.data) {
          this.categories = [...this.categories, res.data];
          this.quizForm.patchValue({ categoryId: res.data.id });
          this.showNewCategory = false;
          this.newCategoryName = '';
          this.newCategoryDesc = '';
          this.toast.success('Category added!');
          this.cdr.detectChanges();
        }
      },
      error: () => this.toast.error('Failed to create category.')
    });
  }

  submitQuiz(): void {
    if (this.quizForm.invalid) { this.quizForm.markAllAsTouched(); return; }
    this.quizLoading = true;
    const val = this.quizForm.value;
    const payload = {
      ...val,
      categoryId: Number(val.categoryId),
      timeLimit: val.timeLimit ? Number(val.timeLimit) : null
    };
    this.quizService.createQuiz(payload).subscribe({
      next: res => {
        this.quizLoading = false;
        if (res.success && res.data) {
          this.toast.success('Quiz created!');
          // Auto-assign the new quiz to this group
          this.groupService.assignQuiz(this.groupId, res.data.id).subscribe({
            next: () => {
              // Mark as requiring validation since GM created it
              this.groupService.setRequiresValidation(this.groupId, res.data!.id).subscribe();
              this.toast.success('Quiz created & assigned to group!');
              this.quizForm.reset({ difficulty: 'Medium' });
              this.loadGroupQuizzes();
              this.activeTab = 'quizzes';
              this.cdr.detectChanges();
              this.router.navigate(['/group-manager/quiz', res.data!.id, 'questions']);
            }
          });
        } else {
          this.toast.error(res.message);
        }
        this.cdr.detectChanges();
      },
      error: () => { this.quizLoading = false; this.toast.error('Failed to create quiz.'); }
    });
  }

  // Validation
  validate(submission: GroupQuizResultDTO, status: string): void {
    this.groupService.validateSubmission(submission.id, status).subscribe({
      next: res => {
        if (res.success) {
          this.toast.success(`Marked as ${status}.`);
          submission.validationStatus = status;
          this.cdr.detectChanges();
        } else {
          this.toast.error(res.message);
        }
      },
      error: () => this.toast.error('Failed to update status.')
    });
  }
}
