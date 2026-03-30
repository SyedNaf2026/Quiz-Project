import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Navbar } from '../../navbar/navbar';
import { QuizAttemptService } from '../../service/quiz-attempt.service';
import { GroupService } from '../../service/group.service';
import { ToastService } from '../../service/toast.service';
import { QuizResultDTO, GroupQuizResultDTO } from '../../models/models';
import jsPDF from 'jspdf';

@Component({
  selector: 'app-my-results',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar],
  templateUrl: './my-results.html'
})
export class MyResults implements OnInit {
  results: QuizResultDTO[] = [];
  groupResults: GroupQuizResultDTO[] = [];
  loading = true;
  groupLoading = true;
  selectedResult: QuizResultDTO | null = null;
  showCertModal = false;
  activeTab: 'individual' | 'group' = 'individual';

  constructor(
    private attemptService: QuizAttemptService,
    private groupService: GroupService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  get totalAttempts(): number { return this.results.length; }
  get avgScore(): number {
    if (!this.results.length) return 0;
    return Math.round(this.results.reduce((s, r) => s + r.percentage, 0) / this.results.length);
  }
  get bestScore(): number {
    if (!this.results.length) return 0;
    return Math.round(Math.max(...this.results.map(r => r.percentage)));
  }
  get certUserName(): string { return localStorage.getItem('user-name') || 'Quiz Taker'; }
  get certDate(): string {
    if (!this.selectedResult) return '';
    return new Date(this.selectedResult.completedAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
  }

  ngOnInit(): void {
    this.attemptService.getMyResults().subscribe({
      next: (res) => { this.results = res.data || []; this.loading = false; this.cdr.detectChanges(); },
      error: () => { this.loading = false; this.cdr.detectChanges(); this.toast.error('Failed to load results.'); }
    });
    this.groupService.getMyGroupResults().subscribe({
      next: (res) => { this.groupResults = res.data || []; this.groupLoading = false; this.cdr.detectChanges(); },
      error: () => { this.groupLoading = false; this.cdr.detectChanges(); }
    });
  }

  openCertificate(r: QuizResultDTO): void {
    this.selectedResult = r;
    this.showCertModal = true;
    this.cdr.detectChanges();
  }

  closeCertificate(): void {
    this.showCertModal = false;
    this.selectedResult = null;
  }

  downloadCertificate(r: QuizResultDTO): void {
    const userName = localStorage.getItem('user-name') || 'Quiz Taker';
    const date = new Date(r.completedAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
    const score = r.score + ' / ' + r.totalQuestions;
    const pct = Math.round(r.percentage);

    const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' });
    const W = 297; const H = 210;

    // Cream background
    doc.setFillColor(253, 250, 245);
    doc.rect(0, 0, W, H, 'F');

    // Outer gold border
    doc.setDrawColor(201, 168, 76);
    doc.setLineWidth(1.5);
    doc.rect(8, 8, W - 16, H - 16);
    // Inner border
    doc.setLineWidth(0.5);
    doc.rect(12, 12, W - 24, H - 24);
    // Innermost thin border
    doc.setLineWidth(0.3);
    doc.setDrawColor(201, 168, 76, 0.4);
    doc.rect(16, 16, W - 32, H - 32);

    // Title
    doc.setFont('times', 'bold');
    doc.setFontSize(32);
    doc.setTextColor(26, 42, 74);
    doc.text('Certificate of Completion', W / 2, 45, { align: 'center' });

    // Gold ornament line
    doc.setDrawColor(201, 168, 76);
    doc.setLineWidth(0.8);
    doc.line(W / 2 - 40, 52, W / 2 + 40, 52);
    doc.setFontSize(10);
    doc.setTextColor(201, 168, 76);
    doc.text('✦  ✦  ✦', W / 2, 58, { align: 'center' });

    // Presented to
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.setTextColor(85, 85, 85);
    doc.setCharSpace(2);
    doc.text('THIS CERTIFICATE IS PROUDLY PRESENTED TO', W / 2, 70, { align: 'center' });
    doc.setCharSpace(0);

    // Name
    doc.setFont('times', 'bold');
    doc.setFontSize(28);
    doc.setTextColor(26, 42, 74);
    doc.text(userName, W / 2, 85, { align: 'center' });

    // Underline name
    const nameWidth = doc.getTextWidth(userName);
    doc.setDrawColor(201, 168, 76);
    doc.setLineWidth(0.8);
    doc.line(W / 2 - nameWidth / 2, 88, W / 2 + nameWidth / 2, 88);

    // Description
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(11);
    doc.setTextColor(68, 68, 68);
    doc.text('For successfully completing the quiz', W / 2, 100, { align: 'center' });

    // Quiz title
    doc.setFont('times', 'bold');
    doc.setFontSize(14);
    doc.setTextColor(26, 42, 74);
    doc.text('"' + r.quizTitle + '"', W / 2, 112, { align: 'center' });

    // Score
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(10);
    doc.setTextColor(102, 102, 102);
    doc.text('Score: ', W / 2 - 20, 124, { align: 'right' });
    doc.setTextColor(201, 168, 76);
    doc.setFont('helvetica', 'bold');
    doc.text(score, W / 2 - 18, 124);
    doc.setTextColor(102, 102, 102);
    doc.setFont('helvetica', 'normal');
    doc.text('  |  Percentage: ', W / 2 - 5, 124);
    doc.setTextColor(201, 168, 76);
    doc.setFont('helvetica', 'bold');
    doc.text(pct + '%', W / 2 + 32, 124);

    // Footer line
    doc.setDrawColor(180, 180, 180);
    doc.setLineWidth(0.3);
    doc.line(30, 175, 110, 175);
    doc.line(W - 110, 175, W - 30, 175);

    // Date
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.setTextColor(51, 51, 51);
    doc.text(date, 70, 172, { align: 'center' });
    doc.setFontSize(7);
    doc.setTextColor(119, 119, 119);
    doc.setCharSpace(1);
    doc.text('DATE', 70, 180, { align: 'center' });
    doc.setCharSpace(0);

    // Issued by
    doc.setFont('times', 'bold');
    doc.setFontSize(14);
    doc.setTextColor(201, 168, 76);
    doc.text('QuizApp', W - 70, 172, { align: 'center' });
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(7);
    doc.setTextColor(119, 119, 119);
    doc.setCharSpace(1);
    doc.text('ISSUED BY', W - 70, 180, { align: 'center' });
    doc.setCharSpace(0);

    doc.save('Certificate-' + r.quizTitle.replace(/\s+/g, '-') + '.pdf');
  }
}