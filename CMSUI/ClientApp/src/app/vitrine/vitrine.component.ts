import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { VitrineAdminService, AreaSite, VitrineAreaConfigResumo } from '../vitrine-admin/vitrine-admin.service';

interface VitrineTemplate {
  vitrineTemplateId: string;
  nome: string;
  descricao: string | null;
  segmentoTenantId: string | null;
  variaveisJson: string;
  htmlCss: string | null;
  thumbnailUrl: string | null;
  dataCriacao: string;
}

interface VitrineSlotBloco {
  tipo: 'texto' | 'imagem' | 'cta' | 'lista';
  config: Record<string, any>;
  ordem: number;
}

interface VitrineValores {
  variaveis: Record<string, string>;
  slots: Record<string, VitrineSlotBloco[]>;
}

@Component({ templateUrl: './vitrine.component.html' })
export class VitrineComponent implements OnInit {
  areaId: string = '';
  areaConfig: VitrineAreaConfigResumo | null = null;
  templates: VitrineTemplate[] = [];
  templateSelecionado: VitrineTemplate | null = null;
  templateSelecionadoId: string = '';
  valores: VitrineValores = { variaveis: {}, slots: {} };
  slotNomes: string[] = [];
  slotAtivo: string | null = null;
  salvando = false;
  publicando = false;
  carregando = false;
  gerando = false;
  erroGeracao: string | null = null;
  prompt = '';
  modoEditor = false;
  previewSrc: SafeResourceUrl | null = null;
  areasSite: AreaSite[] = [];

  readonly tiposDisponiveis = [
    { val: 'landing', label: 'Landing' },
    { val: 'institucional', label: 'Institucional' },
    { val: 'catalogo', label: 'Catálogo' },
    { val: 'portfolio', label: 'Portfólio' }
  ];
  readonly estilosDisponiveis = [
    { val: 'moderno', label: 'Moderno' },
    { val: 'minimalista', label: 'Minimalista' },
    { val: 'bold', label: 'Bold' }
  ];
  readonly paletasDisponiveis = [
    { val: 'dark', label: 'Dark' },
    { val: 'pastel', label: 'Pastel' },
    { val: 'corporativo', label: 'Corporativo' }
  ];

  tipoSelecionado = '';
  estiloSelecionado = '';
  paletaSelecionada = '';

  private _blobUrl: string | null = null;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private sanitizer: DomSanitizer,
    private route: ActivatedRoute,
    private vitrineService: VitrineAdminService
  ) {}

  ngOnInit() {
    this.areaId = this.route.snapshot.paramMap.get('areaid') ?? '';
    this.carregando = true;
    this.http.get<VitrineAreaConfigResumo>(this.baseUrl + 'vitrine/area/' + this.areaId + '/configurada').subscribe({
      next: config => {
        this.areaConfig = config;
        if (config.valoresJson) {
          try {
            const parsed = JSON.parse(config.valoresJson);
            this.valores = { variaveis: parsed.variaveis || {}, slots: parsed.slots || {} };
          } catch {
            this.valores = { variaveis: {}, slots: {} };
          }
        }
        if (config.templateHtmlCss) {
          this.slotNomes = this.extrairSlots(config.templateHtmlCss);
        }
        this.carregarTemplates(config.vitrineTemplateId);
      },
      error: () => { this.carregando = false; }
    });

    const usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    const aplicacaoid = usuario.aplicacaoid as string | undefined;
    if (aplicacaoid) {
      this.vitrineService.getSiteAreas(aplicacaoid).subscribe({
        next: areas => { this.areasSite = areas; },
        error: () => {}
      });
    }
  }

  carregarTemplates(templateAtualId: string | null) {
    this.http.get<VitrineTemplate[]>(this.baseUrl + 'vitrine/templates').subscribe({
      next: templates => {
        this.templates = templates;
        this.carregando = false;
        if (templateAtualId) {
          const atual = templates.find(t => t.vitrineTemplateId === templateAtualId);
          if (atual) {
            this.templateSelecionadoId = atual.vitrineTemplateId;
            this.selecionarTemplate(atual);
          }
        }
      },
      error: () => { this.carregando = false; }
    });
  }

  extrairSlots(html: string): string[] {
    const regex = /data-vitrine-slot="([^"]+)"/g;
    const slots: string[] = [];
    let match;
    while ((match = regex.exec(html)) !== null) {
      if (!slots.includes(match[1])) slots.push(match[1]);
    }
    return slots;
  }

  selecionarTemplate(template: VitrineTemplate) {
    this.templateSelecionado = template;
    if (template.htmlCss) {
      this.slotNomes = this.extrairSlots(template.htmlCss);
    }
    for (const slot of this.slotNomes) {
      if (!this.valores.slots[slot]) {
        this.valores.slots[slot] = [];
      }
    }
    this.previewSrc = null;
  }

  onTemplateIdChange(id: string) {
    if (!id) return;
    this.http.get<VitrineTemplate>(this.baseUrl + 'vitrine/templates/' + id).subscribe({
      next: t => this.selecionarTemplate(t),
      error: () => {}
    });
  }

  adicionarBloco(slotNome: string, tipo: string) {
    if (!this.valores.slots[slotNome]) {
      this.valores.slots[slotNome] = [];
    }
    const arr = this.valores.slots[slotNome];
    let config: Record<string, any>;
    switch (tipo) {
      case 'texto':  config = { texto: '', tag: 'p' }; break;
      case 'imagem': config = { url: '', alt: '' }; break;
      case 'cta':    config = { texto: '', url: '#', variante: 'primario' }; break;
      case 'lista':  config = { itens: [''] }; break;
      default:       config = {};
    }
    arr.push({ tipo: tipo as VitrineSlotBloco['tipo'], config, ordem: arr.length });
  }

  removerBloco(slotNome: string, index: number) {
    const arr = this.valores.slots[slotNome];
    if (!arr) return;
    arr.splice(index, 1);
    arr.forEach((b, i) => b.ordem = i);
  }

  salvar() {
    if (!this.templateSelecionado) return;
    this.salvando = true;
    this.http.put(this.baseUrl + 'vitrine/area/' + this.areaId + '/configurada', {
      vitrineTemplateId: this.templateSelecionado.vitrineTemplateId,
      valoresJson: JSON.stringify(this.valores)
    }).subscribe({
      next: () => { this.salvando = false; },
      error: () => { this.salvando = false; }
    });
  }

  preview() {
    if (!this.templateSelecionado) return;
    this.http.put(this.baseUrl + 'vitrine/area/' + this.areaId + '/configurada', {
      vitrineTemplateId: this.templateSelecionado.vitrineTemplateId,
      valoresJson: JSON.stringify(this.valores)
    }).subscribe({
      next: () => {
        this.http.get(this.baseUrl + 'vitrine/area/' + this.areaId + '/render', { responseType: 'text' }).subscribe({
          next: html => this.definirPreview(html)
        });
      }
    });
  }

  publicar() {
    if (!this.templateSelecionado) return;
    this.publicando = true;
    this.http.put(this.baseUrl + 'vitrine/area/' + this.areaId + '/configurada', {
      vitrineTemplateId: this.templateSelecionado.vitrineTemplateId,
      valoresJson: JSON.stringify(this.valores)
    }).subscribe({
      next: () => {
        this.http.post(this.baseUrl + 'vitrine/area/' + this.areaId + '/publicar', {}).subscribe({
          next: () => {
            this.publicando = false;
            if (this.areaConfig) this.areaConfig.publicado = true;
          },
          error: () => { this.publicando = false; }
        });
      },
      error: () => { this.publicando = false; }
    });
  }

  gerarComIA() {
    if (!this.prompt.trim() || this.gerando) return;
    this.gerando = true;
    this.erroGeracao = null;

    this._buildTemaCanonicoJson().subscribe(temaCanonicoJson => {
      this.vitrineService.gerarArea(this.areaId, {
        prompt: this.prompt,
        tipo: this.tipoSelecionado || undefined,
        estilo: this.estiloSelecionado || undefined,
        paleta: this.paletaSelecionada || undefined,
        temaCanonicoJson: temaCanonicoJson || undefined
      }).subscribe({
        next: () => {
          this.gerando = false;
          this.vitrineService.renderArea(this.areaId).subscribe({
            next: html => this.definirPreview(html)
          });
        },
        error: err => {
          this.gerando = false;
          this.erroGeracao = err?.error?.message || 'Erro ao gerar vitrine. Tente novamente.';
        }
      });
    });
  }

  publicarDireto() {
    this.publicando = true;
    this.http.post(this.baseUrl + 'vitrine/area/' + this.areaId + '/publicar', {}).subscribe({
      next: () => {
        this.publicando = false;
        if (this.areaConfig) this.areaConfig.publicado = true;
      },
      error: () => { this.publicando = false; }
    });
  }

  private _buildTemaCanonicoJson(): Observable<string | null> {
    const canonical = this.areasSite.find(a => a.canonicalArea);
    if (!canonical) return of(null);
    if (canonical.areaid === this.areaId) {
      return of(this._extrairTema(this.areaConfig?.valoresJson));
    }
    return this.vitrineService.getAreaConfig(canonical.areaid).pipe(
      map(cfg => this._extrairTema(cfg.valoresJson)),
      catchError(() => of(null))
    );
  }

  private _extrairTema(valoresJson: string | null | undefined): string | null {
    if (!valoresJson) return null;
    try {
      const parsed = JSON.parse(valoresJson);
      return parsed.tema ? JSON.stringify(parsed.tema) : null;
    } catch {
      return null;
    }
  }

  private definirPreview(html: string) {
    if (this._blobUrl) URL.revokeObjectURL(this._blobUrl);
    const blob = new Blob([html], { type: 'text/html' });
    this._blobUrl = URL.createObjectURL(blob);
    this.previewSrc = this.sanitizer.bypassSecurityTrustResourceUrl(this._blobUrl);
  }
}
