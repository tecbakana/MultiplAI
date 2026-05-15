import { Component, Inject, OnInit, OnDestroy, Renderer2 } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeHtml, SafeResourceUrl } from '@angular/platform-browser';
import { HttpClient } from '@angular/common/http';
import { StringService } from '../services/string.service';

@Component({ templateUrl: './site.component.html' })
export class SiteComponent implements OnInit, OnDestroy {
  site: any = null;
  carregando = true;
  erro = '';
  modo: 'slug' | 'preview' = 'slug';
  areaAtualUrl = '';
  currentYear = new Date().getFullYear();

  slug: string | null = null;
  previewId: string | null = null;

  private _timerInterval: any = null;
  private _contadores: Map<string, any> = new Map();
  private _vitrineLink: HTMLLinkElement | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private sanitizer: DomSanitizer,
    @Inject('BASE_URL') private baseUrl: string,
    private stringService: StringService,
    private renderer: Renderer2
  ) {}

  ngOnInit() {
    this.slug = this.route.snapshot.paramMap.get('slug');
    this.previewId = this.route.snapshot.paramMap.get('id');
    const areaParam = this.route.snapshot.paramMap.get('area') ?? '';
    this.areaAtualUrl = this.stringService.decode(areaParam);
    //this.areaAtualUrl = this.stringService.decode(this.route.snapshot.paramMap.get('area') ?? '');

    const url = this.slug
      ? `${this.baseUrl}site/slug/${this.slug}`
      : `${this.baseUrl}site/preview/${this.previewId}`;

    this.modo = this.slug ? 'slug' : 'preview';

    this.http.get<any>(url).subscribe({
      next: r => {
        this.site = r;
        this.carregando = false;
        if (!this.areaAtualUrl) {
          const areas: any[] = r.areas ?? [];

          
          const home = areas.find((a: any) => a.url?.toLowerCase() === 'home' && a.temLayout);
          const primeira = areas.find((a: any) => a.temLayout);
          console.log('[area areas]', areas);
          this.route.paramMap.subscribe(params => {
            const a = params.get('area');
            console.log('[area paramMap]',a);
            if (a) this.areaAtualUrl = a;
          });
        }
      },
      error: e => {
        this.erro = e.status === 404 ? 'Site não encontrado.' : 'Erro ao carregar o site.';
        this.carregando = false;
      }
    });

    // Atualiza área ao navegar sem recarregar o componente
    this.route.paramMap.subscribe(params => {
      const a = params.get('area');
      if (a) this.areaAtualUrl = a;
    });
  }

  getAreaAtual(): any {
    const areas: any[] = this.site?.areas ?? [];
    if (!this.areaAtualUrl) return areas.find(a => a.temLayout || a.htmlSnapshot) ?? null;
    return areas.find(a => a.url === this.areaAtualUrl)
        ?? areas.find(a => a.temLayout || a.htmlSnapshot)
        ?? null;
  }

  getAreaHtml(): SafeHtml | null {
    const html = this.getAreaAtual()?.htmlSnapshot;
    if (!html) return null;
    return this.sanitizer.bypassSecurityTrustHtml(this._injetarCssVitrine(html));
  }

  private _injetarCssVitrine(html: string): string {
    const match = html.match(/<link[^>]+href="(\/vitrine\/css\/[^"]+)"[^>]*>/i);
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
    // Extrai todos os blocos menu-navegacao de todas as áreas (usado como global)
    for (const area of (this.site?.areas ?? [])) {
      const menu = (area.blocos ?? []).find((b: any) => b.tipo === 'menu-navegacao');
      if (menu) return [menu];
    }
    return [];
  }

  getBlocosConteudo(): any[] {
    return (this.getAreaAtual()?.blocos ?? []).filter((b: any) => b.tipo !== 'menu-navegacao');
  }

  private static FULL_BLEED = new Set(['hero', 'hero-cta', 'banner-imagem', 'contador', 'rodape']);

  private _linhasCachedArea = '';
  private _linhas: { blocos: any[]; fullBleed: boolean }[] = [];

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
      if (SiteComponent.FULL_BLEED.has(bloco.tipo)) {
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
    return this.getBlocosConteudo().some(b => b.tipo === 'rodape');
  }

  navegarArea(url: string) {
    this.areaAtualUrl = url;
    if (this.slug) this.router.navigate(['/s', this.slug, url]);
    else if (this.previewId) this.router.navigate(['/preview', this.previewId, url]);
  }

  parseCampos(valor: string): any[] {
    try { return JSON.parse(valor) ?? []; } catch { return []; }
  }

  getVideoUrl(url: string): SafeResourceUrl {
    let embedUrl = url;
    // YouTube
    const ytMatch = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/)([^&?/]+)/);
    if (ytMatch) embedUrl = `https://www.youtube.com/embed/${ytMatch[1]}`;
    // Vimeo
    const vimeoMatch = url.match(/vimeo\.com\/(\d+)/);
    if (vimeoMatch) embedUrl = `https://player.vimeo.com/video/${vimeoMatch[1]}`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
  }

  getContador(dataAlvo: string): any {
    if (!dataAlvo) return { encerrado: true };
    const key = dataAlvo;
    if (!this._contadores.has(key)) {
      this._contadores.set(key, this._calcContador(dataAlvo));
      if (!this._timerInterval) {
        this._timerInterval = setInterval(() => {
          this._contadores.forEach((_, k) => {
            this._contadores.set(k, this._calcContador(k));
          });
        }, 1000);
      }
    }
    return this._contadores.get(key);
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
  }
}
