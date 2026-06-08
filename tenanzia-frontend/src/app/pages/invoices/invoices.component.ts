import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InvoicesService } from '../../services/invoices.service';
import { AuthService } from '../../services/auth.service';
import jsPDF from 'jspdf';
import { ThemeService } from '../../services/theme.service';
import { NotificationBellComponent } from '../../components/notification-bell/notification-bell.component';
import { SubscriptionService } from '../../services/subscription.service';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, FormsModule, NotificationBellComponent],
  templateUrl: './invoices.component.html',
})
export class InvoicesComponent implements OnInit {
  sending: { [key: number]: boolean } = {};
showMenu = false;

  invoices: any[] = [];
  filteredInvoices: any[] = [];
  loading = true;
  selectedStatus = '';
  searchText = '';
  curuntPlan: any = null;

  stats = {
    total: 0,
    paid: 0,
    unpaid: 0,
    cancelled: 0,
    totalCollected: 0
  };
  username = '';
  isOwner = false;
  isManager = false;
  isEmployee = false;
  isManagerOrOwner = false;


  constructor(
    private invoicesService: InvoicesService,
    private authService: AuthService,
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
    this.loadInvoices();
    this.getCurrentPlan();
  }

  loadInvoices() {
    this.loading = true;
    this.invoicesService.getAll(this.selectedStatus).subscribe({
      next: (res) => {
        this.invoices = res;
        this.calculateStats();
        this.applySearch();
        this.loading = false;
      },
      error: () => this.authService.logout()
    });
  }

  calculateStats() {
    this.stats = {
      total: this.invoices.length,
      paid: this.invoices.filter(i => i.status === 'Paid').length,
      unpaid: this.invoices.filter(i => i.status === 'Unpaid').length,
      cancelled: this.invoices.filter(i => i.status === 'Cancelled').length,
      totalCollected: this.invoices
        .filter(i => i.status === 'Paid')
        .reduce((sum, i) => sum + i.amount, 0)
    };
  }

  applySearch() {
    if (!this.searchText) {
      this.filteredInvoices = this.invoices;
      return;
    }
    this.filteredInvoices = this.invoices.filter(i =>
      i.customerName.toLowerCase().includes(this.searchText.toLowerCase())
    );
  }

  filterByStatus(status: string) {
    this.selectedStatus = status;
    this.loadInvoices();
  }

  markAsPaid(id: number) {
    this.invoicesService.markAsPaid(id).subscribe({
      next: () => this.loadInvoices()
    });
  }

  cancel(id: number) {
    this.invoicesService.cancel(id).subscribe({
      next: () => this.loadInvoices()
    });
  }

  delete(id: number) {
    if (confirm('Delete this invoice?')) {
      this.invoicesService.delete(id).subscribe({
        next: () => this.loadInvoices()
      });
    }
  }

  getStatusClass(status: string) {
    if (status === 'Paid') return 'bg-[#12251E] text-[#5DCAA5]';
    if (status === 'Unpaid') return 'bg-[#2A1F0A] text-[#EF9F27]';
    return 'bg-[#1E1E24] text-[#666]';
  }

  logout() { this.authService.logout(); }
  goTo(page: string) { this.router.navigate([`/${page}`]); }




  downloadPdf(invoice: any) {
    const doc = new jsPDF();

    // Colors
    const pink = [212, 83, 126] as [number, number, number];
    const dark = [17, 17, 20] as [number, number, number];
    const gray = [102, 102, 102] as [number, number, number];

    // Background
    doc.setFillColor(...dark);
    doc.rect(0, 0, 210, 297, 'F');

    // Header Bar
    doc.setFillColor(...pink);
    doc.rect(0, 0, 210, 40, 'F');

    // Logo Text
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(22);
    doc.setFont('helvetica', 'bold');
    doc.text('Tenanzia', 20, 25);

    // Invoice Title
    doc.setFontSize(12);
    doc.setFont('helvetica', 'normal');
    doc.text('INVOICE', 160, 20);
    doc.setFontSize(10);
    doc.text(`#${invoice.id}`, 160, 28);

    // Company Info
    doc.setTextColor(...gray);
    doc.setFontSize(9);
    doc.text('Invoice Details', 20, 55);

    doc.setTextColor(240, 240, 242);
    doc.setFontSize(10);
    doc.text(`Customer: ${invoice.customerName}`, 20, 65);
    doc.text(`Order: #${invoice.orderId}`, 20, 73);
    doc.text(`Issued: ${new Date(invoice.issuedAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}`, 20, 81);

    if (invoice.paidAt) {
      doc.text(`Paid: ${new Date(invoice.paidAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}`, 20, 89);
    }

    // Status Badge
    const statusColor = invoice.status === 'Paid'
      ? [15, 110, 86] as [number, number, number]
      : [133, 79, 11] as [number, number, number];

    doc.setFillColor(...statusColor);
    doc.roundedRect(150, 58, 40, 10, 2, 2, 'F');
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(9);
    doc.text(invoice.status.toUpperCase(), 170, 65, { align: 'center' });

    // Divider
    doc.setDrawColor(...pink);
    doc.setLineWidth(0.5);
    doc.line(20, 100, 190, 100);

    // Items Table Header
    doc.setTextColor(...gray);
    doc.setFontSize(9);
    doc.text('DESCRIPTION', 20, 112);
    doc.text('QTY', 120, 112);
    doc.text('UNIT PRICE', 145, 112);
    doc.text('TOTAL', 175, 112);

    doc.setDrawColor(30, 30, 36);
    doc.line(20, 115, 190, 115);

    // Items
    let y = 125;
    doc.setTextColor(240, 240, 242);
    doc.setFontSize(10);

    if (invoice.items && invoice.items.length > 0) {
      invoice.items.forEach((item: any) => {
        doc.text(item.productName, 20, y);
        doc.text(item.quantity.toString(), 120, y);
        doc.text(`$${item.unitPrice.toLocaleString()}`, 145, y);
        doc.text(`$${(item.quantity * item.unitPrice).toLocaleString()}`, 175, y);
        y += 10;
      });
    } else {
      doc.text('Service/Product', 20, y);
      doc.text('1', 120, y);
      doc.text(`$${invoice.amount.toLocaleString()}`, 145, y);
      doc.text(`$${invoice.amount.toLocaleString()}`, 175, y);
      y += 10;
    }

    // Total
    doc.setDrawColor(...pink);
    doc.line(20, y + 5, 190, y + 5);

    doc.setFillColor(30, 30, 36);
    doc.rect(130, y + 8, 60, 14, 'F');

    doc.setTextColor(...gray);
    doc.setFontSize(9);
    doc.text('TOTAL', 140, y + 17);

    doc.setTextColor(...pink);
    doc.setFontSize(14);
    doc.setFont('helvetica', 'bold');
    doc.text(`$${invoice.amount.toLocaleString()}`, 185, y + 17, { align: 'right' });

    // Footer
    doc.setFont('helvetica', 'normal');
    doc.setFillColor(...pink);
    doc.rect(0, 275, 210, 22, 'F');

    doc.setTextColor(255, 255, 255);
    doc.setFontSize(9);
    doc.text('Thank you for your business!', 105, 284, { align: 'center' });
    doc.setTextColor(255, 200, 220);
    doc.setFontSize(8);
    doc.text('Generated by Tenanzia — Business Management Platform', 105, 291, { align: 'center' });

    // Save
    doc.save(`Invoice_${invoice.id}_${invoice.customerName}.pdf`);
  }

  sendInvoice(invoice: any) {
    if (!invoice.customerName) {
      alert('Customer has no email!');
      return;
    }

    this.sending[invoice.id] = true;

    this.invoicesService.sendInvoice(invoice.id).subscribe({
      next: () => {
        this.sending[invoice.id] = false;
        alert(`Invoice #${invoice.id} sent to ${invoice.customerName}!`);
      },
      error: (err) => {
        this.sending[invoice.id] = false;
        alert('Failed to send: ' + err.error);
      }
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