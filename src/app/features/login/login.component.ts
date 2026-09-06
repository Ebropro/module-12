import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'

})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  username = '';
  password = '';

  loading = signal(false);
  errorMessage = signal('');

  async login(): Promise<void> {
    if (!this.username || !this.password) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    try {
      await this.authService.login({
        email: this.username,
        password: this.password,
      });

      const role = this.authService.currentUser()?.role;

if (role === 'Admin') {
  await this.router.navigate(['/admin/courses']);
} else if (role === 'Instructor') {
  await this.router.navigate(['/instructor']);
} else {
  await this.router.navigate(['/dashboard']);
}

    } catch {
      this.errorMessage.set('Invalid username or password.');
    } finally {
      this.loading.set(false);
    }
  }
}