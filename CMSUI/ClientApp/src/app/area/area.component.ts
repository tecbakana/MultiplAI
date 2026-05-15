import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Router } from '@angular/router';
import { AdminContextService } from '../admin-context.service';

@Component({ templateUrl: './area.component.html' })
export class AreaComponent implements OnInit {
  lista: any[] = [];
  selecionado: any = null;
  modoEdicao = false;
  private usuario: any;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private adminCtx: AdminContextService,
    private router: Router
  ) {}

  ngOnInit() {
    this.usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    this.adminCtx.tenant$.subscribe(() => this.carregar());
  }

  private params(): HttpParams {
    let p = new HttpParams();
    if (!this.usuario.acessoTotal && this.usuario.aplicacaoid)
      p = p.set('aplicacaoid', this.usuario.aplicacaoid);
    else if (this.usuario.acessoTotal && this.adminCtx.tenantId)
      p = p.set('aplicacaoid', this.adminCtx.tenantId);
    return p;
  }

  carregar() {
    this.http.get<any[]>(this.baseUrl + 'areas', { params: this.params() })
      .subscribe(r => this.lista = r);
  }

  nomePai(areaidpai: string | null): string {
    if (!areaidpai) return '—';
    const pai = this.lista.find(a => a.areaid === areaidpai);
    return pai ? pai.nome : areaidpai;
  }

  opcoesPai(): any[] {
    if (!this.selecionado?.areaid) return this.lista;
    return this.lista.filter(a => a.areaid !== this.selecionado.areaid);
  }

  jaTemHome(): boolean {
    return this.lista.some(a => a.tipo === 'home' && a.areaid !== this.selecionado?.areaid);
  }

  novo() {
    this.selecionado = {
      nome: '', url: '', descricao: '', areaidpai: null, posicao: null, tipoarea: null,
      tipo: 'pagina',
      aplicacaoid: this.usuario.aplicacaoid
    };
    this.modoEdicao = true;
  }

  editar(item: any) { this.selecionado = { ...item }; this.modoEdicao = true; }

  salvar() {
    if (this.selecionado.areaid) {
      this.http.put(this.baseUrl + 'areas/' + this.selecionado.areaid, this.selecionado)
        .subscribe({ next: () => { this.modoEdicao = false; this.carregar(); }, error: e => alert(e.error || 'Erro ao salvar') });
    } else {
      this.http.post(this.baseUrl + 'areas', this.selecionado)
        .subscribe({ next: () => { this.modoEdicao = false; this.carregar(); }, error: e => alert(e.error || 'Erro ao salvar') });
    }
  }

  excluir(id: string) {
    if (confirm('Excluir esta área?'))
      this.http.delete(this.baseUrl + 'areas/' + id).subscribe(() => this.carregar());
  }

  cancelar() { this.modoEdicao = false; this.selecionado = null; }

  navegarParaEditor(item: any) {
    if (item.tipo === 'home') {
      this.router.navigate(['/vitrine', item.areaid]);
    } else {
      this.router.navigate(['/page-builder-v2', item.areaid]);
    }
  }
}
