import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({ templateUrl: './segmento.component.html' })
export class SegmentoComponent implements OnInit {
  lista: any[] = [];
  selecionado: any = null;
  modoEdicao = false;

  templates: any[] = [];
  segmentoTemplates: string | null = null;
  carregandoTemplates = false;

  promptSegmento = '';
  gerando = false;
  geradoCount = 0;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.http.get<any[]>(this.baseUrl + 'segmentos').subscribe(r => this.lista = r);
  }

  novo() {
    this.selecionado = { nome: '', descricao: '' };
    this.modoEdicao = true;
    this.segmentoTemplates = null;
    this.templates = [];
  }

  editar(item: any) {
    this.selecionado = { ...item };
    this.modoEdicao = true;
    this.segmentoTemplates = null;
    this.templates = [];
  }

  salvar() {
    const payload = { nome: this.selecionado.nome, descricao: this.selecionado.descricao };
    if (this.selecionado.segmentoTenantId) {
      this.http.put(this.baseUrl + `segmentos/${this.selecionado.segmentoTenantId}`, payload)
        .subscribe(() => { this.modoEdicao = false; this.carregar(); });
    } else {
      this.http.post(this.baseUrl + 'segmentos', payload)
        .subscribe(() => { this.modoEdicao = false; this.carregar(); });
    }
  }

  excluir(id: string) {
    if (confirm('Remover este segmento?')) {
      this.http.delete(this.baseUrl + `segmentos/${id}`).subscribe(() => this.carregar());
    }
  }

  cancelar() {
    this.modoEdicao = false;
    this.selecionado = null;
    this.segmentoTemplates = null;
    this.templates = [];
  }

  abrirTemplates(item: any) {
    this.selecionado = { ...item };
    this.segmentoTemplates = item.segmentoTenantId;
    this.modoEdicao = false;
    this.carregarTemplates(item.segmentoTenantId);
  }

  carregarTemplates(segmentoId: string) {
    this.carregandoTemplates = true;
    this.http.get<any[]>(this.baseUrl + `segmentos/${segmentoId}/templates`)
      .subscribe({ next: r => { this.templates = r; this.carregandoTemplates = false; }, error: () => this.carregandoTemplates = false });
  }

  gerarTemplates() {
    if (!this.promptSegmento.trim() || !this.segmentoTemplates) return;
    this.gerando = true;
    this.geradoCount = 0;
    this.http.post<any>(
      this.baseUrl + `segmentos/${this.segmentoTemplates}/templates/gerar`,
      { promptSegmento: this.promptSegmento }
    ).subscribe({
      next: r => {
        this.gerando = false;
        this.geradoCount = r.gerados;
        this.promptSegmento = '';
        this.carregarTemplates(this.segmentoTemplates!);
      },
      error: () => { this.gerando = false; }
    });
  }

  fecharTemplates() {
    this.segmentoTemplates = null;
    this.templates = [];
    this.selecionado = null;
  }
}
