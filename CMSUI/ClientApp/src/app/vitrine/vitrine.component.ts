import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

interface VitrineVariavel {
  chave: string;
  label: string;
  tipo: 'color' | 'font' | 'text' | 'image' | 'number';
  padrao: string | null;
}

interface VitrineTemplate {
  vitrineTemplateId: string;
  nome: string;
  descricao: string | null;
  segmentoTenantId: string | null;
  variaveisJson: string;
  thumbnailUrl: string | null;
  dataCriacao: string;
}

interface VitrineConfigurada {
  vitrineConfiguradaId: string;
  aplicacaoId: string;
  vitrineTemplateId: string;
  valoresJson: string;
  publicado: boolean;
}

@Component({ templateUrl: './vitrine.component.html' })
export class VitrineComponent implements OnInit {
  templates: VitrineTemplate[] = [];
  templateSelecionado: VitrineTemplate | null = null;
  variaveis: VitrineVariavel[] = [];
  valores: { [chave: string]: string } = {};
  configurada: VitrineConfigurada | null = null;
  previewSrc: SafeResourceUrl | null = null;
  salvando = false;
  publicando = false;
  carregando = false;
  areaId: string | null = null;

  readonly fontes = ['Playfair Display', 'Lato', 'Roboto', 'Montserrat', 'Open Sans', 'Poppins', 'Inter', 'Nunito'];

  private _blobUrl: string | null = null;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private sanitizer: DomSanitizer,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.areaId = this.route.snapshot.queryParamMap.get('areaId');
    this.carregando = true;
    this.http.get<VitrineConfigurada | null>(this.baseUrl + 'vitrine/configurada').subscribe({
      next: conf => {
        this.configurada = conf;
        if (conf?.valoresJson) {
          try { this.valores = JSON.parse(conf.valoresJson); } catch { this.valores = {}; }
        }
        this.carregarTemplates(conf?.vitrineTemplateId ?? null);
      },
      error: () => { this.carregando = false; }
    });
  }

  carregarTemplates(templateAtualId: string | null) {
    this.http.get<VitrineTemplate[]>(this.baseUrl + 'vitrine/templates').subscribe({
      next: templates => {
        this.templates = templates;
        this.carregando = false;
        if (templateAtualId) {
          const atual = templates.find(t => t.vitrineTemplateId === templateAtualId);
          if (atual) this.selecionarTemplate(atual, false);
        }
      },
      error: () => { this.carregando = false; }
    });
  }

  selecionarTemplate(template: VitrineTemplate, resetarValores = true) {
    this.templateSelecionado = template;
    try {
      this.variaveis = JSON.parse(template.variaveisJson) || [];
    } catch {
      this.variaveis = [];
    }
    if (resetarValores) {
      this.valores = {};
      for (const v of this.variaveis) {
        if (v.padrao) this.valores[v.chave] = v.padrao;
      }
    }
    this.previewSrc = null;
  }

  previsualizarAsync() {
    if (!this.templateSelecionado) return;
    const payload = {
      vitrineTemplateId: this.templateSelecionado.vitrineTemplateId,
      valoresJson: JSON.stringify(this.valores)
    };
    this.http.put(this.baseUrl + 'vitrine/configurada', payload).subscribe({
      next: () => {
        const renderUrl = this.areaId
          ? this.baseUrl + 'vitrine/render?areaId=' + encodeURIComponent(this.areaId)
          : this.baseUrl + 'vitrine/render';
        this.http.get(renderUrl, { responseType: 'text' }).subscribe({
          next: html => this.definirPreview(html)
        });
      }
    });
  }

  salvarRascunho() {
    if (!this.templateSelecionado) return;
    this.salvando = true;
    const payload = {
      vitrineTemplateId: this.templateSelecionado.vitrineTemplateId,
      valoresJson: JSON.stringify(this.valores)
    };
    this.http.put(this.baseUrl + 'vitrine/configurada', payload).subscribe({
      next: () => { this.salvando = false; },
      error: () => { this.salvando = false; }
    });
  }

  publicar() {
    if (!this.templateSelecionado) return;
    this.publicando = true;
    this.http.put(this.baseUrl + 'vitrine/configurada', {
      vitrineTemplateId: this.templateSelecionado.vitrineTemplateId,
      valoresJson: JSON.stringify(this.valores)
    }).subscribe({
      next: () => {
        this.http.post(this.baseUrl + 'vitrine/publicar', {}).subscribe({
          next: () => {
            this.publicando = false;
            if (this.configurada) this.configurada.publicado = true;
          },
          error: () => { this.publicando = false; }
        });
      },
      error: () => { this.publicando = false; }
    });
  }

  private definirPreview(html: string) {
    if (this._blobUrl) URL.revokeObjectURL(this._blobUrl);
    const blob = new Blob([html], { type: 'text/html' });
    this._blobUrl = URL.createObjectURL(blob);
    this.previewSrc = this.sanitizer.bypassSecurityTrustResourceUrl(this._blobUrl);
  }
}
