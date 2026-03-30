import { Routes } from '@angular/router';
import { authGuard } from './auth-guard';
import { roleGuard } from './role-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  // Auth
  { path: 'login', loadComponent: () => import('./login/login').then(m => m.Login) },
  { path: 'register', loadComponent: () => import('./register/register').then(m => m.Register) },

  // Profile (all roles)
  {
    path: 'profile',
    loadComponent: () => import('./profile/profile').then(m => m.Profile),
    canActivate: [authGuard]
  },

  // Admin routes
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard(['Admin'])],
    children: [
      { path: 'dashboard', loadComponent: () => import('./admin/dashboard/admin-dashboard').then(m => m.AdminDashboard) },
      { path: 'categories', loadComponent: () => import('./admin/categories/admin-categories').then(m => m.AdminCategories) },
      { path: 'users', loadComponent: () => import('./admin/users/admin-users').then(m => m.AdminUsers) },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // Creator routes
  {
    path: 'creator',
    canActivate: [authGuard, roleGuard(['QuizCreator'])],
    children: [
      { path: 'my-quizzes', loadComponent: () => import('./creator/my-quizzes/my-quizzes').then(m => m.MyQuizzes) },
      { path: 'create-quiz', loadComponent: () => import('./creator/create-quiz/create-quiz').then(m => m.CreateQuiz) },
      { path: 'edit-quiz/:id', loadComponent: () => import('./creator/create-quiz/create-quiz').then(m => m.CreateQuiz) },
      { path: 'quiz/:id/questions', loadComponent: () => import('./creator/quiz-questions/quiz-questions').then(m => m.QuizQuestions) },
      { path: '', redirectTo: 'my-quizzes', pathMatch: 'full' }
    ]
  },

  // Taker routes
  {
    path: 'taker',
    canActivate: [authGuard, roleGuard(['QuizTaker'])],
    children: [
      { path: 'browse', loadComponent: () => import('./taker/browse/browse-quizzes').then(m => m.BrowseQuizzes) },
      { path: 'take/:id', loadComponent: () => import('./taker/take-quiz/take-quiz').then(m => m.TakeQuiz) },
      { path: 'results', loadComponent: () => import('./taker/results/my-results').then(m => m.MyResults) },
      { path: 'leaderboard', loadComponent: () => import('./taker/leaderboard/leaderboard').then(m => m.Leaderboard) },
      { path: '', redirectTo: 'browse', pathMatch: 'full' }
    ]
  },

  // Group Manager routes
  {
    path: 'group-manager',
    canActivate: [authGuard, roleGuard(['GroupManager'])],
    children: [
      { path: 'dashboard', loadComponent: () => import('./group-manager/dashboard/gm-dashboard').then(m => m.GmDashboard) },
      { path: 'group/:id', loadComponent: () => import('./group-manager/group-detail/group-detail').then(m => m.GroupDetail) },
      { path: 'quiz/:id/questions', loadComponent: () => import('./group-manager/quiz-questions/gm-quiz-questions').then(m => m.GmQuizQuestions) },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // Legacy home redirect
  { path: 'home', loadComponent: () => import('./home/home').then(m => m.Home), canActivate: [authGuard] },

  // Catch-all
  { path: '**', redirectTo: 'login' }
];
