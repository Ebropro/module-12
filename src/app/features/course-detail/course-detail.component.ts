import {
  Component,
  ChangeDetectionStrategy,
  inject,
  input,
  signal,
  effect,
} from '@angular/core';

import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

import { CourseService } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { Course } from '../../models/course.model';

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [RouterLink, CommonModule],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './course-detail.component.html',
})
export class CourseDetailComponent {

  private courseService = inject(CourseService);
  private authService = inject(AuthService);

  id = input.required<string>();

  course = signal<Course | null>(null);
  loading = signal(true);
  notFound = signal(false);

  isLoggedIn = () =>
    this.authService.currentUser() !== null;

  constructor() {
    effect(() => {
      const courseId = Number(this.id());

      if (!courseId) {
        this.notFound.set(true);
        this.loading.set(false);
        return;
      }

      this.loading.set(true);
      this.notFound.set(false);

      this.courseService.getById(courseId).subscribe({
        next: course => {
          this.course.set(course);
          this.notFound.set(course === null);
          this.loading.set(false);
        },

        error: error => {
          console.error(
            'Failed to load course:',
            error
          );

          this.course.set(null);
          this.notFound.set(true);
          this.loading.set(false);
        }
      });
    });
  }
}