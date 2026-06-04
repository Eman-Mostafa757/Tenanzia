import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationsService } from '../../services/notifications.service';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-bell.component.html',
})
export class NotificationBellComponent implements OnInit {

  notifications: any[] = [];
  unreadCount = 0;
  showDropdown = false;

  constructor(private notificationsService: NotificationsService) {}

  ngOnInit() {
    this.loadUnreadCount();
    // Poll كل 30 ثانية
    setInterval(() => this.loadUnreadCount(), 30000);
  }

  loadUnreadCount() {
    this.notificationsService.getUnreadCount().subscribe({
      next: (res) => this.unreadCount = res.count
    });
  }

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
    if (this.showDropdown) this.loadNotifications();
  }

  loadNotifications() {
    this.notificationsService.getAll().subscribe({
      next: (res) => this.notifications = res
    });
  }

  markAsRead(id: number) {
    this.notificationsService.markAsRead(id).subscribe({
      next: () => {
        const n = this.notifications.find(n => n.id === id);
        if (n) n.isRead = true;
        this.unreadCount = Math.max(0, this.unreadCount - 1);
      }
    });
  }

  markAllAsRead() {
    this.notificationsService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
      }
    });
  }

  getTypeIcon(type: string) {
    if (type === 'task') return '✅';
    if (type === 'order') return '🛒';
    if (type === 'invoice') return '💰';
    if (type === 'comment') return '💬';
    return '🔔';
  }

  getTimeAgo(date: string) {
    const now = new Date();
    const then = new Date(date);
    const diff = Math.floor((now.getTime() - then.getTime()) / 1000);
    if (diff < 60) return 'Just now';
    if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
    return `${Math.floor(diff / 86400)}d ago`;
  }
}