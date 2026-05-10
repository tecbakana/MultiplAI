import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, NavigationEnd } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AdminContextService } from '../admin-context.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html'
})
export class NavMenuComponent implements OnInit, OnDestroy {
  isExpanded = false;
  adminExpanded = true;
  isDark = false;
  modulos: any[] = [];
  usuario: any = null;
  aplicacoes: any[] = [];
  tenantSelecionadoId = '';

  private routerSub?: Subscription;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    public adminCtx: AdminContextService,
    private router: Router
  ) {}

  ngOnInit() {
    const saved = localStorage.getItem('theme') || 'light';
    this.isDark = saved === 'dark';
    document.documentElement.setAttribute('data-theme', saved);

    this.carregarUsuario();

    this.routerSub = this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => this.carregarUsuario());
  }

  ngOnDestroy() {
    this.routerSub?.unsubscribe();
  }

  private carregarUsuario() {
    const novoUsuario = JSON.parse(sessionStorage.getItem('usuario') || 'null');
    const mudou = JSON.stringify(novoUsuario) !== JSON.stringify(this.usuario);
    if (!mudou) return;

    this.usuario = novoUsuario;
    this.modulos = [];
    this.aplicacoes = [];

    if (this.usuario) {
      this.http.get<any[]>(this.baseUrl + 'modulos').subscribe(m => this.modulos = m);

      if (this.usuario.acessoTotal) {
        this.http.get<any[]>(this.baseUrl + 'aplicacaos').subscribe(a => this.aplicacoes = a);
        this.tenantSelecionadoId = this.adminCtx.tenantId ?? '';
      }
    }
  }

  onTenantChange() {
    const app = this.aplicacoes.find(a => a.aplicacaoid === this.tenantSelecionadoId);
    this.adminCtx.set(app ? { aplicacaoid: app.aplicacaoid, nome: app.nome } : null);
  }

  toggleTheme() {
    this.isDark = !this.isDark;
    const theme = this.isDark ? 'dark' : 'light';
    localStorage.setItem('theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
  }

  toggle() { this.isExpanded = !this.isExpanded; }

  logout() {
    sessionStorage.removeItem('usuario');
    sessionStorage.removeItem('adminTenant');
    window.location.href = '/login';
  }
}
