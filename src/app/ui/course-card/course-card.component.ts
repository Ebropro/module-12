import { Component, input, output, computed } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Course } from "../../models/course.model";

const COLORS = ['bg-blue-500', 'bg-purple-500', 'bg-green-500', 'bg-amber-500', 'bg-rose-500', 'bg-cyan-500'];

@Component({
  selector: "tms-course-card",
  standalone: true,
  imports: [RouterLink],
  templateUrl: "./course-card.component.html",
  styleUrl: "./course-card.component.scss",
})
export class CourseCardComponent {
  course = input.required<Course>();
  enrollClicked = output<Course>();

  initials = computed(() => this.course().code.split('-')[0].slice(0, 2));
  badgeColor = computed(() => {
    const hash = this.course().code.split('').reduce((a, c) => a + c.charCodeAt(0), 0);
    return COLORS[hash % COLORS.length];
  });
}