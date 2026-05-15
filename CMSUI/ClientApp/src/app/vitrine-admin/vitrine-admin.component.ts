import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

interface VitrineTemplateResumo {
  vitrineTemplateId: string;
  nome: string;
  descricao: string | null;
  segmentoTenantId: string | null;
  variaveisJson: string;
  thumbnailUrl: string | null;
  dataCriacao: string;
}

interface SegmentoTenant {
  segmentoTenantId: string;
  nome: string;
}

interface VitrineTemplateDetalhe extends VitrineTemplateResumo {
  htmlCss: string;
}

type Modo = 'lista' | 'editar' | 'gerar';

@Component({ templateUrl: './vitrine-admin.component.html' })
export class VitrineAdminComponent implements OnInit {
  modo: Modo = 'lista';
  lista: VitrineTemplateResumo[] = [];
  segmentos: SegmentoTenant[] = [];

  form: Partial<VitrineTemplateDetalhe> = {};
  editandoId: string | null = null;
  salvando = false;

  promptIA = '';
  gerando = false;
  htmlGerado = '';
  variaveisGeradas = '';

  previewSrc: SafeResourceUrl | null = null;
  carregando = false;

  private _blobUrl: string | null = null;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit() {
    this.carregar();
    this.http.get<SegmentoTenant[]>(this.baseUrl + 'Segmentos').subscribe({
      next: r => { this.segmentos = r; },
      error: () => {}
    });
  }

  carregar() {
    this.carregando = true;
    this.http.get<VitrineTemplateResumo[]>(this.baseUrl + 'admin/vitrine/templates').subscribe({
      next: r => { this.lista = r; this.carregando = false; },
      error: () => { this.carregando = false; }
    });
  }

  novo() {
    this.editandoId = null;
    this.form = { nome: '', descricao: '', segmentoTenantId: '', htmlCss: '', variaveisJson: '[]', thumbnailUrl: '' };
    this.previewSrc = null;
    this.modo = 'editar';
  }

  editar(id: string) {
    this.http.get<VitrineTemplateDetalhe>(this.baseUrl + 'admin/vitrine/templates/' + id).subscribe(t => {
      this.editandoId = id;
      this.form = { ...t };
      this.previewSrc = null;
      this.modo = 'editar';
    });
  }

  salvar() {
    if (!this.form.nome?.trim() || !this.form.htmlCss?.trim()) return;
    this.salvando = true;
    const payload = {
      nome: this.form.nome,
      descricao: this.form.descricao || null,
      segmentoTenantId: this.form.segmentoTenantId || null,
      htmlCss: this.form.htmlCss,
      variaveisJson: this.form.variaveisJson || '[]',
      thumbnailUrl: this.form.thumbnailUrl || null
    };
    const req = this.editandoId
      ? this.http.put(this.baseUrl + 'admin/vitrine/templates/' + this.editandoId, payload)
      : this.http.post(this.baseUrl + 'admin/vitrine/templates', payload);
    req.subscribe({
      next: () => { this.salvando = false; this.cancelar(); this.carregar(); },
      error: () => { this.salvando = false; }
    });
  }

  desativar(id: string) {
    if (!confirm('Desativar este template?')) return;
    this.http.delete(this.baseUrl + 'admin/vitrine/templates/' + id).subscribe(() => this.carregar());
  }

  previsualizar() {
    if (this.form.htmlCss) this._setPreview(this.form.htmlCss);
  }

  abrirGerador() {
    this.promptIA = '';
    this.htmlGerado = '';
    this.variaveisGeradas = '';
    this.previewSrc = null;
    this.modo = 'gerar';
  }

  gerarViaIA() {
    if (!this.promptIA.trim()) return;
    this.gerando = true;
    this.htmlGerado = '';
    this.previewSrc = null;
    this.http.post<{ htmlCss: string; variaveisJson: string }>(
      this.baseUrl + 'admin/vitrine/templates/gerar',
      { prompt: this.promptIA, imagemBase64: null, segmentoTenantId: null }
    ).subscribe({
      next: r => {
        this.htmlGerado = r.htmlCss;
        this.variaveisGeradas = r.variaveisJson;
        this._setPreview(r.htmlCss);
        this.gerando = false;
      },
      error: () => { this.gerando = false; }
    });
  }

  usarGerado() {
    this.editandoId = null;
    this.form = {
      nome: '',
      descricao: '',
      segmentoTenantId: '',
      htmlCss: this.htmlGerado,
      variaveisJson: this.variaveisGeradas,
      thumbnailUrl: ''
    };
    this.previewSrc = null;
    this.modo = 'editar';
  }

  cancelar() {
    this.modo = 'lista';
    this.form = {};
    this.editandoId = null;
    this.previewSrc = null;
  }

  private _setPreview(html: string) {
    if (this._blobUrl) URL.revokeObjectURL(this._blobUrl);
    const blob = new Blob([html], { type: 'text/html' });
    this._blobUrl = URL.createObjectURL(blob);
    this.previewSrc = this.sanitizer.bypassSecurityTrustResourceUrl(this._blobUrl);
  }
}
