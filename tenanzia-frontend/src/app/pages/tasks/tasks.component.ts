import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TasksService } from '../../services/tasks.service';
import { AuthService } from '../../services/auth.service';
import { UsersService } from '../../services/users.service';
import { ThemeService } from '../../services/theme.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { SubscriptionService } from '../../services/subscription.service';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, FormsModule, NotificationBellComponent],
  templateUrl: './tasks.component.html',
})
export class TasksComponent implements OnInit {

  kanban: any = { toDo: [], inProgress: [], completed: [], cancelled: [] };
  loading = true;
  showModal = false;
  filterMode = 'all';
  users: any[] = [];

  form = {
    title: '',
    description: '',
    priority: 'Medium',
    dueDate: '',
    assignedToUserId: null as number | null
  };
  showDetailsModal = false;
  selectedTask: any = null;
  newComment = '';
  currentUserId = 0;
  username = '';
  curuntPlan: any = null;
  isOwner = false;
  isManager = false;
  isEmployee = false;
  isManagerOrOwner = false;
showMenu = false;

  constructor(
    private tasksService: TasksService,
    private authService: AuthService,
    private usersService: UsersService,
    private router: Router,
    public themeService: ThemeService,
    private subscriptionService: SubscriptionService,

  ) { }

  ngOnInit() {
    this.isOwner = this.authService.isOwner();
    this.isManager = this.authService.isManager();
    this.isEmployee = this.authService.isEmployee();
    this.isManagerOrOwner = this.authService.isManagerOrOwner();
    this.username = this.authService.getUsername();
    this.getCurrentPlan();
    // حمّلي الـ users دايماً مش بس للـ Manager
    this.usersService.getTenantUsers().subscribe({
      next: (res) => this.users = res
    });

    this.loadKanban();
  }
  getUserName(id: number): string {
    const user = this.users.find(u => u.id === id);
    return user ? user.username : '';
  }

  loadKanban() {
    this.loading = true;
    this.tasksService.getKanban().subscribe({
      next: (res) => {
        this.kanban = res;
        this.loading = false;
      },
      error: () => this.authService.logout()
    });
  }

  createTask() {
    if (!this.form.title) return;
    this.tasksService.create(this.form).subscribe({
      next: () => {
        this.showModal = false;
        this.form = {
          title: '',
          description: '',
          priority: 'Medium',
          dueDate: '',
          assignedToUserId: null  // ← جديد
        };
        this.loadKanban();
      }
    });
  }

  moveTask(task: any, newStatus: string) {
    this.tasksService.updateStatus(task.id, newStatus).subscribe({
      next: () => this.loadKanban()
    });
  }

  deleteTask(id: number) {
    if (confirm('Delete permanently?')) {
      this.tasksService.delete(id).subscribe({
        next: () => this.loadKanban()
      });
    }
  }
  cancelTask(id: number) {
    this.tasksService.updateStatus(id, 'Cancelled').subscribe({
      next: () => this.loadKanban()
    });
  }

  logout() { this.authService.logout(); }
  goTo(page: string) { this.router.navigate([`/${page}`]); }


  openTaskDetails(task: any) {
    this.tasksService.getTaskDetails(task.id).subscribe({
      next: (res) => {
        this.selectedTask = res;
        this.showDetailsModal = true;
      }
    });
  }

  addComment() {
    if (!this.newComment.trim() || !this.selectedTask) return;
    this.tasksService.addComment(this.selectedTask.id, this.newComment).subscribe({
      next: () => {
        this.newComment = '';
        this.openTaskDetails(this.selectedTask);
      }
    });
  }

  deleteComment(commentId: number) {
    if (!this.selectedTask) return;
    this.tasksService.deleteComment(this.selectedTask.id, commentId).subscribe({
      next: () => this.openTaskDetails(this.selectedTask)
    });
  }

  getCurrentPlan() {
    this.subscriptionService.getCurrent().subscribe({
      next: (res) => {
        this.curuntPlan = res;

      }
    })

  }

}