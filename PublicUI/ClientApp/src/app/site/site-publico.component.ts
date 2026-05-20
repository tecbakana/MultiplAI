import { Component, OnInit, OnDestroy, Renderer2 } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeHtml, SafeResourceUrl } from '@angular/platform-browser';
import { HttpClient } from '@angular/common/http';

@Component({ templateUrl: './site-publico.component.html' })
export class SitePublicoComponent implements OnInit, OnDestroy {
  site: any = null;
  carregando = true;
  erro = '';
  areaAtualUrl = '';
  slug = '';
  token = '';
  logoUrl: string | null = null;
  currentYear = new Date().getFullYear();

  private _timerInterval: any = null;
  private _logoObjectUrl: string | null = null;
  private _contadores: Map<string, any> = new Map();
  private _linhasCachedArea = '';
  private _linhas: { blocos: any[]; fullBleed: boolean }[] = [];
  private _vitrineLink: HTMLLinkElement | null = null;
  private _vitrineNavConfig: { corFundo: string; corTexto: string } | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private sanitizer: DomSanitizer,
    private renderer: Renderer2
  ) {}

  ngOnInit() {
    this.slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.areaAtualUrl = this.route.snapshot.paramMap.get('areaurl') ?? '';

    this.http.get<{ token: string }>(`/api/publico/site/resolve?slug=${encodeURIComponent(this.slug)}`).subscribe({
      next: r => {
        this.token = r.token;
        this._carregarLogo();
        this.http.get<any>(`/api/publico/${this.token}/site`).subscribe({
          next: site => {
            this.site = site;
            this.carregando = false;
          },
          error: e => {
            this.erro = e.status === 404 ? 'Site não encontrado.' : 'Erro ao carregar o site.';
            this.carregando = false;
          }
        });
      },
      error: e => {
        this.erro = e.status === 404 ? 'Site não encontrado.' : 'Erro ao carregar o site.';
        this.carregando = false;
      }
    });

    this.route.paramMap.subscribe(params => {
      const a = params.get('areaurl');
      if (a) this.areaAtualUrl = a;
    });
  }

  getAreaAtual(): any {
    const areas: any[] = this.site?.areas ?? [];
    if (!this.areaAtualUrl) return areas.find((a: any) => a.temLayout || a.htmlSnapshot) ?? null;
    return areas.find((a: any) => a.url === this.areaAtualUrl)
        ?? areas.find((a: any) => a.temLayout || a.htmlSnapshot)
        ?? null;
  }

  getAreaHtml(): SafeHtml | null {
    const html = this.getAreaAtual()?.htmlSnapshot;
    if (!html) return null;
    this._atualizarVitrineNavConfig(html);
    return this.sanitizer.bypassSecurityTrustHtml(this._injetarCssVitrine(this._removerNavDoSnapshot(html)));
  }

  private _atualizarVitrineNavConfig(html: string): void {
    const match = html.match(/<style>:root\{([^}]+)\}/i);
    if (!match) { this._vitrineNavConfig = null; return; }
    const vars = match[1];
    const corFundo = vars.match(/--v-cor-fundo:([^;]+)/)?.[1]?.trim() ?? '';
    const corTexto = vars.match(/--v-cor-texto:([^;]+)/)?.[1]?.trim() ?? '';
    this._vitrineNavConfig = { corFundo, corTexto };
  }

  private _removerNavDoSnapshot(html: string): string {
    const div = document.createElement('div');
    div.innerHTML = html;
    div.querySelectorAll('nav').forEach(el => el.remove());
    return div.innerHTML;
  }

  private _injetarCssVitrine(html: string): string {
    const match = html.match(/<link[^>]+href="(\/vitrine\/(?:css\/[^"]+|design-system\.css))"[^>]*>/i);
    if (!match) return html;
    if (!this._vitrineLink) {
      const link: HTMLLinkElement = this.renderer.createElement('link');
      link.rel = 'stylesheet';
      link.href = match[1];
      this.renderer.appendChild(document.head, link);
      this._vitrineLink = link;
    }
    return html.replace(match[0], '');
  }

  getMenuNavegacao(): any[] {
    for (const area of (this.site?.areas ?? [])) {
      const menu = (area.blocos ?? []).find((b: any) => b.tipo === 'menu-navegacao');
      if (menu) return [menu];
    }
    const areas = (this.site?.areas ?? []).filter((a: any) => a.temLayout || a.htmlSnapshot);
    if (areas.length < 2) return [];
    return [{ config: { cor_fundo: this._vitrineNavConfig?.corFundo ?? '', cor_texto: this._vitrineNavConfig?.corTexto ?? '' }, dados: areas.map((a: any) => ({ areaid: a.areaid, nome: a.nome, url: a.url })) }];
  }

  private _carregarLogo(): void {
    this.http.get(`/api/publico/${this.token}/logo?t=${Date.now()}`, { responseType: 'blob' }).subscribe({
      next: blob => {
        if (this._logoObjectUrl) URL.revokeObjectURL(this._logoObjectUrl);
        this._logoObjectUrl = URL.createObjectURL(blob);
        this.logoUrl = this._logoObjectUrl;
      },
      error: () => { this.logoUrl = null; }
    });
  }

  getBlocosConteudo(): any[] {
    return (this.getAreaAtual()?.blocos ?? []).filter((b: any) => b.tipo !== 'menu-navegacao');
  }

  private static FULL_BLEED = new Set(['hero', 'hero-cta', 'banner-imagem', 'contador', 'rodape']);

  getLinhas(): { blocos: any[]; fullBleed: boolean }[] {
    if (this._linhasCachedArea === this.areaAtualUrl && this._linhas.length > 0)
      return this._linhas;
    this._linhasCachedArea = this.areaAtualUrl;

    const result: { blocos: any[]; fullBleed: boolean }[] = [];
    let rowBlocos: any[] = [];
    let rowCols = 0;

    const flush = () => {
      if (rowBlocos.length > 0) { result.push({ blocos: rowBlocos, fullBleed: false }); rowBlocos = []; rowCols = 0; }
    };
    const colSize = (coluna?: string) => coluna === '1/2' ? 6 : coluna === '1/3' ? 4 : (coluna === 'auto' || coluna === 'fill') ? 0 : 12;

    for (const bloco of this.getBlocosConteudo()) {
      if (SitePublicoComponent.FULL_BLEED.has(bloco.tipo)) {
        flush();
        result.push({ blocos: [bloco], fullBleed: true });
      } else {
        const cols = colSize(bloco.coluna);
        if (cols === 12) { flush(); result.push({ blocos: [bloco], fullBleed: false }); }
        else if (rowCols + cols > 12) { flush(); rowBlocos = [bloco]; rowCols = cols; }
        else { rowBlocos.push(bloco); rowCols += cols; }
      }
    }
    flush();
    this._linhas = result;
    return result;
  }

  getColClass(coluna?: string): string {
    if (coluna === '1/2') return 'col-12 col-md-6';
    if (coluna === '1/3') return 'col-12 col-md-4';
    if (coluna === 'auto') return 'col-auto';
    if (coluna === 'fill') return 'col';
    return 'col-12';
  }

  temBlocoRodape(): boolean {
    return this.getBlocosConteudo().some((b: any) => b.tipo === 'rodape');
  }

  navegarArea(url: string) {
    this.areaAtualUrl = url;
    this.router.navigate(['/s', this.slug, url]);
  }

  parseCampos(valor: string): any[] {
    try { return JSON.parse(valor) ?? []; } catch { return []; }
  }

  getVideoUrl(url: string): SafeResourceUrl {
    let embedUrl = url;
    const ytMatch = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/)([^&?/]+)/);
    if (ytMatch) embedUrl = `https://www.youtube.com/embed/${ytMatch[1]}`;
    const vimeoMatch = url.match(/vimeo\.com\/(\d+)/);
    if (vimeoMatch) embedUrl = `https://player.vimeo.com/video/${vimeoMatch[1]}`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
  }

  getContador(dataAlvo: string): any {
    if (!dataAlvo) return { encerrado: true };
    if (!this._contadores.has(dataAlvo)) {
      this._contadores.set(dataAlvo, this._calcContador(dataAlvo));
      if (!this._timerInterval) {
        this._timerInterval = setInterval(() => {
          this._contadores.forEach((_, k) => this._contadores.set(k, this._calcContador(k)));
        }, 1000);
      }
    }
    return this._contadores.get(dataAlvo);
  }

  private _calcContador(dataAlvo: string): any {
    const diff = new Date(dataAlvo).getTime() - Date.now();
    if (diff <= 0) return { encerrado: true };
    const s = Math.floor(diff / 1000);
    return {
      encerrado: false,
      dias: String(Math.floor(s / 86400)).padStart(2, '0'),
      horas: String(Math.floor((s % 86400) / 3600)).padStart(2, '0'),
      minutos: String(Math.floor((s % 3600) / 60)).padStart(2, '0'),
      segundos: String(s % 60).padStart(2, '0')
    };
  }

  ngOnDestroy() {
    if (this._timerInterval) clearInterval(this._timerInterval);
    if (this._vitrineLink) this.renderer.removeChild(document.head, this._vitrineLink);
    if (this._logoObjectUrl) URL.revokeObjectURL(this._logoObjectUrl);
  }
}
