import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Navbar } from '../../navbar/navbar';
import { QuizService } from '../../service/quiz.service';
import { QuestionService } from '../../service/question.service';
import { QuizAttemptService } from '../../service/quiz-attempt.service';
import { ToastService } from '../../service/toast.service';
import { AnswerDTO, QuestionDTO, QuizDTO, QuizResultDTO } from '../../models/models';

@Component({
  selector: 'app-take-quiz',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar],
  templateUrl: './take-quiz.html'
})
export class TakeQuiz implements OnInit, OnDestroy {
  quiz: QuizDTO | null = null;
  questions: QuestionDTO[] = [];
  answers: Map<number, number> = new Map();
  multiAnswers: Map<number, Set<number>> = new Map();
  skipped: Set<number> = new Set();
  currentIndex = 0;
  loading = true;
  loadError = false;
  errorMessage = '';
  submitting = false;
  submitted = false;
  quizStarted = false;
  result: QuizResultDTO | null = null;
  showResult = false;
  timeLeft = 0;
  private timerInterval: any = null;
  private warnedAt30 = false;
  private warnedAt10 = false;

  get currentQuestion(): QuestionDTO | null { return this.questions[this.currentIndex] ?? null; }
  get answeredCount(): number { return this.questions.filter(q => this.isAnswered(q.id)).length; }
  get progress(): number { return this.questions.length ? (this.answeredCount / this.questions.length) * 100 : 0; }

  get timerClass(): string {
    if (!this.quiz?.timeLimit) return 'timer-normal';
    const total = this.quiz.timeLimit * 60;
    if (this.timeLeft <= 10) return 'timer-danger';
    if (this.timeLeft <= 30 || this.timeLeft < total * 0.25) return 'timer-warning';
    return 'timer-normal';
  }

  get timerDisplay(): string {
    const m = Math.floor(this.timeLeft / 60);
    const s = this.timeLeft % 60;
    return m + ':' + s.toString().padStart(2, '0');
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private quizService: QuizService,
    private questionService: QuestionService,
    private attemptService: QuizAttemptService,
    private toast: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.setError('Invalid quiz ID.'); return; }
    this.quizService.getQuizById(id).subscribe({
      next: (res) => {
        if (!res.success || !res.data) { this.setError('Quiz not found.'); return; }
        this.quiz = res.data;
        this.cdr.detectChanges();
        this.questionService.getByQuiz(id).subscribe({
          next: (qRes) => {
            this.questions = qRes.data || [];
            this.loading = false;
            if (this.questions.length === 0) { this.setError('This quiz has no questions yet.'); return; }
            if (this.quiz?.timeLimit) this.timeLeft = this.quiz.timeLimit * 60;
            this.cdr.detectChanges();
          },
          error: (err) => this.setError(err?.error?.message || 'Failed to load questions.')
        });
      },
      error: (err) => this.setError(err?.error?.message || 'Failed to load quiz.')
    });
  }

  private setError(msg: string): void {
    this.loading = false; this.loadError = true; this.errorMessage = msg; this.cdr.detectChanges();
  }

  ngOnDestroy(): void { if (this.timerInterval) clearInterval(this.timerInterval); }

  confirmStart(): void {
    this.quizStarted = true; this.currentIndex = 0; this.cdr.detectChanges();
    setTimeout(() => { if (this.quiz?.timeLimit) this.startTimer(); }, 100);
  }

  startTimer(): void {
    this.timerInterval = setInterval(() => {
      this.timeLeft--;
      this.cdr.detectChanges();
      if (this.timeLeft === 30 && !this.warnedAt30) { this.warnedAt30 = true; this.toast.error('⚠️ 30 seconds remaining!'); }
      if (this.timeLeft === 10 && !this.warnedAt10) { this.warnedAt10 = true; this.toast.error('🚨 10 seconds left — hurry!'); }
      if (this.timeLeft <= 0) {
        clearInterval(this.timerInterval); this.timerInterval = null;
        this.toast.error('⏰ Time is up! Auto-submitting...'); this.submit();
      }
    }, 1000);
  }

  goToQuestion(index: number): void { if (this.submitted) return; this.currentIndex = index; this.cdr.detectChanges(); }

  skipQuestion(): void {
    if (!this.currentQuestion) return;
    this.skipped.add(this.currentQuestion.id);
    const next = this.findNext();
    if (next !== -1) this.currentIndex = next;
    this.cdr.detectChanges();
  }

  private findNext(): number {
    for (let i = this.currentIndex + 1; i < this.questions.length; i++) {
      if (!this.isAnswered(this.questions[i].id) && !this.skipped.has(this.questions[i].id)) return i;
    }
    for (let i = 0; i < this.currentIndex; i++) {
      if (!this.isAnswered(this.questions[i].id) && !this.skipped.has(this.questions[i].id)) return i;
    }
    return this.currentIndex + 1 < this.questions.length ? this.currentIndex + 1 : this.currentIndex;
  }

  questionStatus(q: QuestionDTO): 'answered' | 'skipped' | 'unanswered' | 'active' {
    if (this.questions[this.currentIndex]?.id === q.id && !this.submitted) return 'active';
    if (this.isAnswered(q.id)) return 'answered';
    if (this.skipped.has(q.id)) return 'skipped';
    return 'unanswered';
  }

  isAnswered(questionId: number): boolean {
    const q = this.questions.find(x => x.id === questionId);
    if (!q) return false;
    if (q.questionType === 'MultipleAnswer') return (this.multiAnswers.get(questionId)?.size ?? 0) > 0;
    return this.answers.has(questionId);
  }

  selectAnswer(questionId: number, optionId: number): void {
    if (this.submitted) return;
    this.skipped.delete(questionId); this.answers.set(questionId, optionId); this.cdr.detectChanges();
  }

  isSelected(questionId: number, optionId: number): boolean { return this.answers.get(questionId) === optionId; }

  toggleMultiAnswer(questionId: number, optionId: number): void {
    if (this.submitted) return;
    this.skipped.delete(questionId);
    if (!this.multiAnswers.has(questionId)) this.multiAnswers.set(questionId, new Set());
    const set = this.multiAnswers.get(questionId)!;
    if (set.has(optionId)) set.delete(optionId); else set.add(optionId);
    this.cdr.detectChanges();
  }

  isMultiSelected(questionId: number, optionId: number): boolean {
    return this.multiAnswers.get(questionId)?.has(optionId) ?? false;
  }

  wasSelected(questionId: number, optionId: number): boolean { return this.answers.get(questionId) === optionId; }

  wasMultiSelected(questionId: number, optionId: number): boolean {
    const bd = this.result?.answerBreakdown.find(a => a.questionId === questionId);
    return bd?.selectedOptionIds?.includes(optionId) ?? false;
  }

  isCorrectOption(questionId: number, optionId: number): boolean {
    const bd = this.result?.answerBreakdown.find(a => a.questionId === questionId);
    if (!bd) return false;
    if (bd.questionType === 'MultipleAnswer') return bd.correctOptionIds?.includes(optionId) ?? false;
    return bd.correctOptionId === optionId;
  }

  optionClass(q: QuestionDTO, optionId: number): string {
    if (!this.submitted) {
      if (q.questionType === 'MultipleAnswer') return this.isMultiSelected(q.id, optionId) ? 'btn-primary' : 'btn-outline';
      return this.isSelected(q.id, optionId) ? 'btn-primary' : 'btn-outline';
    }
    const correct = this.isCorrectOption(q.id, optionId);
    const selected = q.questionType === 'MultipleAnswer' ? this.wasMultiSelected(q.id, optionId) : this.wasSelected(q.id, optionId);
    if (correct) return 'btn-correct';
    if (selected && !correct) return 'btn-wrong';
    return 'btn-outline';
  }

  downloadCertificate(): void {
    if (!this.result) return;
    const userName = localStorage.getItem('user-name') || 'Quiz Taker';
    const date = new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
    const score = this.result.score + ' / ' + this.result.totalQuestions;
    const pct = Math.round(this.result.percentage);
    const quizTitle = this.result.quizTitle;

    const html = '<!DOCTYPE html><html><head><meta charset="UTF-8"><title>Certificate</title>' +
      '<style>' +
      '@import url(\'https://fonts.googleapis.com/css2?family=Cinzel:wght@700&family=Lato:wght@400;700&display=swap\');' +
      '* { margin:0; padding:0; box-sizing:border-box; }' +
      'body { background:#f0ede8; display:flex; align-items:center; justify-content:center; min-height:100vh; font-family:Lato,sans-serif; }' +
      '.cert { width:900px; height:640px; background:#fdfaf5; border:2px solid #c9a84c; position:relative; padding:50px 70px; display:flex; flex-direction:column; align-items:center; justify-content:center; text-align:center; box-shadow:0 8px 40px rgba(0,0,0,0.18); }' +
      '.cert::before { content:""; position:absolute; inset:10px; border:1px solid #c9a84c; pointer-events:none; }' +
      '.cert::after { content:""; position:absolute; inset:18px; border:1px solid rgba(201,168,76,0.3); pointer-events:none; }' +
      '.cert-title { font-family:Cinzel,serif; font-size:2.6rem; color:#1a2a4a; margin-bottom:6px; }' +
      '.ornament { color:#c9a84c; font-size:1.2rem; margin-bottom:24px; letter-spacing:6px; }' +
      '.presented { font-size:0.8rem; letter-spacing:3px; color:#555; text-transform:uppercase; margin-bottom:8px; }' +
      '.name { font-family:Cinzel,serif; font-size:2rem; color:#1a2a4a; border-bottom:2px solid #c9a84c; padding-bottom:6px; margin-bottom:16px; min-width:400px; }' +
      '.desc { font-size:0.95rem; color:#444; margin-bottom:6px; }' +
      '.quiz-name { font-family:Cinzel,serif; font-size:1.1rem; color:#1a2a4a; margin-bottom:6px; }' +
      '.score-line { font-size:0.9rem; color:#666; margin-bottom:28px; }' +
      '.score-line strong { color:#c9a84c; font-size:1.1rem; }' +
      '.footer { display:flex; justify-content:space-between; width:100%; margin-top:auto; padding-top:20px; }' +
      '.footer-item { text-align:center; min-width:180px; }' +
      '.footer-line { border-top:1px solid #999; padding-top:6px; font-size:0.78rem; color:#777; letter-spacing:1px; text-transform:uppercase; }' +
      '.footer-value { font-size:0.9rem; color:#333; margin-bottom:4px; }' +
      '.seal { width:60px; height:60px; background:linear-gradient(135deg,#4f46e5,#7c3aed); border-radius:50%; display:flex; align-items:center; justify-content:center; color:#fff; font-size:1.5rem; font-weight:700; }' +
      '@media print { body { background:white; } .cert { box-shadow:none; } }' +
      '</style></head><body>' +
      '<div class="cert">' +
      '<div class="cert-title">Certificate of Completion</div>' +
      '<div class="ornament">&#10022; &#10022; &#10022;</div>' +
      '<div class="presented">This Certificate is Proudly Presented to</div>' +
      '<div class="name">' + userName + '</div>' +
      '<div class="desc">For successfully completing the quiz</div>' +
      '<div class="quiz-name">&ldquo;' + quizTitle + '&rdquo;</div>' +
      '<div class="score-line">Score: <strong>' + score + '</strong> &nbsp;|&nbsp; Percentage: <strong>' + pct + '%</strong></div>' +
      '<div class="footer">' +
      '<div class="footer-item"><div class="footer-value">' + date + '</div><div class="footer-line">Date</div></div>' +
      '<div></div>' +
      '<div class="footer-item"><div style="font-size:1.4rem;color:#c9a84c;font-family:Cinzel,serif">QuizApp</div><div class="footer-line">Issued By</div></div>' +
      '</div></div>' +
      '<script>window.onload=function(){window.print();}<\/script>' +
      '</body></html>';

    const win = window.open('', '_blank', 'width=960,height=700');
    if (win) { win.document.write(html); win.document.close(); }
  }

  submit(): void {
    if (!this.quiz || this.submitting || this.submitted) return;
    if (this.timerInterval) { clearInterval(this.timerInterval); this.timerInterval = null; }
    this.submitting = true;
    this.cdr.detectChanges();
    const answersArr: AnswerDTO[] = this.questions.map(q => {
      if (q.questionType === 'MultipleAnswer') {
        return { questionId: q.id, selectedOptionId: 0, selectedOptionIds: Array.from(this.multiAnswers.get(q.id) ?? []) };
      }
      return { questionId: q.id, selectedOptionId: this.answers.get(q.id) ?? 0, selectedOptionIds: [] };
    });
    this.attemptService.submitQuiz({ quizId: this.quiz.id, answers: answersArr }).subscribe({
      next: (res) => {
        this.submitting = false;
        this.submitted = true;
        if (res.success && res.data) {
          this.result = res.data;
          this.cdr.detectChanges();
          setTimeout(() => { this.showResult = true; this.cdr.detectChanges(); }, 1800);
        } else {
          this.toast.error(res.message || 'Submission failed.');
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        this.submitting = false;
        this.submitted = false;
        this.toast.error(err?.error?.message || err?.error?.Message || 'Submission failed.');
        this.cdr.detectChanges();
      }
    });
  }
}