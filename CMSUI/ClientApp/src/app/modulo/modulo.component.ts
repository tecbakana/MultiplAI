import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({ templateUrl: './modulo.component.html' })
export class ModuloComponent implements OnInit {
  lista: any[] = [];
  selecionado: any = null;
  modoEdicao = false;
  erro = '';
  usuario: any;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {}

  ngOnInit() {
    this.usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    this.carregar();
  }

  carregar() {
    this.http.get<any[]>(this.baseUrl + 'modulos').subscribe(r => this.lista = r);
  }

  novo() {
    this.selecionado = { nome: '', url: '', posicao: null };
    this.modoEdicao = true;
    this.erro = '';
  }

  editar(item: any) {
    this.selecionado = { ...item };
    this.modoEdicao = true;
    this.erro = '';
  }

  salvar() {
    const body = { nome: this.selecionado.nome, url: this.selecionado.url, posicao: this.selecionado.posicao };
    const req = this.selecionado.moduloid
      ? this.http.put(this.baseUrl + 'modulos/' + this.selecionado.moduloid, body)
      : this.http.post(this.baseUrl + 'modulos', body);

    req.subscribe({
      next: () => { this.modoEdicao = false; this.selecionado = null; this.erro = ''; this.carregar(); },
      error: err => this.erro = err.error?.message || 'Erro ao salvar.'
    });
  }

  excluir(id: string) {
    if (confirm('Excluir este módulo?'))
      this.http.delete(this.baseUrl + 'modulos/' + id).subscribe({
        next: () => this.carregar(),
        error: err => this.erro = err.error?.message || 'Erro ao excluir.'
      });
  }

  cancelar() { this.modoEdicao = false; this.selecionado = null; this.erro = ''; }
}
