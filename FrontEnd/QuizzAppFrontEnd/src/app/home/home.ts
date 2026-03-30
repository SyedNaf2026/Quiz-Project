import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Navbar } from '../navbar/navbar';
import { AuthService } from '../service/auth.service';

const QUOTES = [
  { text: 'The more that you read, the more things you will know.', author: 'Dr. Seuss' },
  { text: 'An investment in knowledge pays the best interest.', author: 'Benjamin Franklin' },
  { text: 'Education is the most powerful weapon you can use to change the world.', author: 'Nelson Mandela' },
  { text: 'Live as if you were to die tomorrow. Learn as if you were to live forever.', author: 'Mahatma Gandhi' },
  { text: 'The beautiful thing about learning is that no one can take it away from you.', author: 'B.B. King' },
  { text: 'Knowledge is power. Information is liberating.', author: 'Kofi Annan' },
  { text: 'Curiosity is the engine of achievement.', author: 'Ken Robinson' },
  { text: 'The expert in anything was once a beginner.', author: 'Helen Hayes' },
  { text: 'Learning never exhausts the mind.', author: 'Leonardo da Vinci' },
  { text: 'It does not matter how slowly you go as long as you do not stop.', author: 'Confucius' },
];

const FEATURES = [
  { icon: '⏱️', title: 'Timed Quizzes', desc: 'Every quiz has a countdown timer. Get warned at 30s and 10s — stay sharp!' },
  { icon: '⚡', title: 'Instant Feedback', desc: 'See correct and wrong answers highlighted immediately after submitting.' },
  { icon: '🎚️', title: 'Difficulty Levels', desc: 'Quizzes are tagged Easy, Medium or Hard. Filter by difficulty to match your level.' },
  { icon: '🏆', title: 'Leaderboard', desc: 'Compete with others and see where you rank on the global leaderboard.' },
  { icon: '📊', title: 'Result Breakdown', desc: 'Detailed answer breakdown after every quiz so you know what to improve.' },
  { icon: '🌙', title: 'Dark Mode', desc: 'Easy on the eyes — switch between light and dark mode anytime.' },
];

const FAQS = [
  { q: 'How do I start a quiz?', a: 'Go to Browse Quizzes, pick a quiz and click Start Quiz. A confirmation screen shows quiz details before the timer begins.' },
  { q: 'Can I retake a quiz?', a: 'Yes! You can retake any quiz. Your previous attempt is replaced with the new one.' },
  { q: 'How is my score calculated?', a: 'Each correct answer gives 1 point. Your percentage is (correct / total) × 100.' },
  { q: 'What happens when the timer runs out?', a: 'The quiz auto-submits with whatever answers you have selected so far.' },
  { q: 'How do I create a quiz?', a: 'Log in as a QuizCreator, go to My Quizzes → Create Quiz, fill in the details and add questions.' },
  { q: 'Can I add my own categories?', a: 'Yes — while creating a quiz, click "+ New" next to the category dropdown to add a new category inline.' },
  { q: 'Where can I see my past results?', a: 'Go to My Results from the navigation bar to see all your quiz attempts and scores.' },
  { q: 'Is there a leaderboard?', a: 'Yes! The Leaderboard shows top scores across all quizzes. Try to reach #1!' },
];

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, Navbar],
  templateUrl: './home.html'
})
export class Home implements OnInit, OnDestroy {
  role = '';
  userName = '';
  quote = QUOTES[0];
  features = FEATURES;
  faqs = FAQS;
  openFaq: number | null = null;
  private quoteInterval: any;

  constructor(private auth: AuthService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.role = this.auth.getRole() || '';
    this.userName = this.auth.getUserName() || '';
    this.randomQuote();
    // Rotate quote every 8 seconds
    this.quoteInterval = setInterval(() => {
      this.randomQuote();
      this.cdr.detectChanges();
    }, 8000);
  }

  ngOnDestroy(): void {
    if (this.quoteInterval) clearInterval(this.quoteInterval);
  }

  randomQuote(): void {
    const idx = Math.floor(Math.random() * QUOTES.length);
    this.quote = QUOTES[idx];
  }

  nextQuote(): void {
    this.randomQuote();
    this.cdr.detectChanges();
  }

  toggleFaq(i: number): void {
    this.openFaq = this.openFaq === i ? null : i;
    this.cdr.detectChanges();
  }

  get isCreator(): boolean { return this.role === 'QuizCreator'; }
  get isTaker(): boolean { return this.role === 'QuizTaker'; }
  get isGroupManager(): boolean { return this.role === 'GroupManager'; }
}
