import { Component, ElementRef, Inject, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

@Component({ templateUrl: './aplicacao.component.html' })
export class AplicacaoComponent implements OnInit {
  lista: any[] = [];
  selecionado: any = null;
  modoEdicao = false;
  abaAtiva = 'geral';

  readonly layouts = [
    { valor: '_Layout.cshtml',      label: 'Básico I' },
    { valor: '_LayoutBasic.cshtml', label: 'Básico II' },
    { valor: '_LayoutFlame.cshtml', label: 'OnePage' },
    { valor: '_LayoutLoja.cshtml',  label: 'Loja' }
  ];

  minhApp = false;
  tokens: any[] = [];
  novoTokenVencimento = '';

  readonly marketplaces = [
    { id: 'mercadolivre', nome: 'Mercado Livre', icone: 'bi-bag-check' },
    { id: 'shopee',       nome: 'Shopee',         icone: 'bi-cart3' },
    { id: 'magalu',       nome: 'Magazine Luiza', icone: 'bi-building-fill-check' },
    { id: 'b2w',          nome: 'B2W / Americanas', icone: 'bi-shop-window' },
  ];

  configuracoes: any[] = [];
  marketplaceSelecionado: string | null = null;
  credenciais: any = {};
  salvandoConfig = false;
  configSalva = false;
  mlConectado = false;
  mlErro = false;
  conectandoMl = false;

  segmentosDisponiveis: any[] = [];
  meusSegmentos: string[] = [];

  logoUrl: string | null = null;
  erroLogo: string | null = null;
  private _logoObjectUrl: string | null = null;

  @ViewChild('logoInput') logoInput!: ElementRef<HTMLInputElement>;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.modoEdicao = false;
    this.selecionado = null;
    this.minhApp = this.route.snapshot.routeConfig?.path === 'minha-aplicacao';
    this.mlConectado = this.route.snapshot.queryParams['ml_connected'] === '1';
    this.mlErro = this.route.snapshot.queryParams['ml_error'] === '1';
    if (this.mlConectado || this.mlErro) {
      this.router.navigate([], { queryParams: {}, replaceUrl: true });
    }
    this.carregar();
  }

  carregar() {
    this.http.get<any[]>(this.baseUrl + 'aplicacaos').subscribe(r => {
      if (this.minhApp) {
        const usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
        this.lista = r.filter(a => a.aplicacaoid === usuario.aplicacaoid);
        // se só tem uma, já abre em modo edição diretamente
        if (this.lista.length === 1) this.editar(this.lista[0]);
      } else {
        this.lista = r;
      }
    });
  }

  novo() {
    this.selecionado = {
      nome: '', url: '', layoutchoose: '_Layout.cshtml', isactive: true,
      mailuser: '', mailpassword: '', mailserver: '', mailport: 587, issecure: false,
      pagsegurotoken: '', ogleadsense: '', header: '',
      pagefacebook: '', pageinstagram: '', pagetwitter: '',
      pagelinkedin: '', pagepinterest: '', pageflicker: '',
      telefone: '', endereco: '', descricao: ''
    };
    this.abaAtiva = 'geral';
    this.modoEdicao = true;
  }

  editar(item: any) {
    this.selecionado = { ...item, issecure: item.issecure === 1 || item.issecure === true };
    this.abaAtiva = 'geral';
    this.modoEdicao = true;
    this.configuracoes = [];
    this.marketplaceSelecionado = null;
    if (item.aplicacaoid) this.carregarTokens(item.aplicacaoid);
    this.carregarLogo();
  }

  carregarTokens(aplicacaoid: string) {
    this.http.get<any[]>(this.baseUrl + `publictokens?aplicacaoid=${aplicacaoid}`)
      .subscribe(r => this.tokens = r);
  }

  gerarToken() {
    const payload: any = { aplicacaoid: this.selecionado.aplicacaoid };
    if (this.novoTokenVencimento) payload.datavencimento = this.novoTokenVencimento;
    this.http.post(this.baseUrl + 'publictokens', payload)
      .subscribe(() => { this.novoTokenVencimento = ''; this.carregarTokens(this.selecionado.aplicacaoid); });
  }

  revogarToken(id: string) {
    if (confirm('Revogar este token? Links públicos que usam este token deixarão de funcionar.')) {
      this.http.delete(this.baseUrl + `publictokens/${id}`)
        .subscribe(() => this.carregarTokens(this.selecionado.aplicacaoid));
    }
  }

  copiarToken(token: string) {
    navigator.clipboard.writeText(token);
  }

  salvar() {
    const usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    const payload = { ...this.selecionado, idusuarioinicio: usuario.userid };

    if (this.selecionado.aplicacaoid) {
      this.http.put(this.baseUrl + 'aplicacaos/' + this.selecionado.aplicacaoid, payload)
        .subscribe(() => { this.modoEdicao = false; this.carregar(); });
    } else {
      this.http.post(this.baseUrl + 'aplicacaos', payload)
        .subscribe(() => { this.modoEdicao = false; this.carregar(); });
    }
  }

  alternarStatus(item: any) {
    const novoStatus = !item.isactive;
    const msg = novoStatus ? 'Ativar esta aplicação?' : 'Inativar esta aplicação?';
    if (confirm(msg)) {
      this.http.patch(this.baseUrl + 'aplicacaos/' + item.aplicacaoid + '/status', novoStatus)
        .subscribe(() => this.carregar());
    }
  }

  excluir(id: string) {
    if (confirm('Excluir esta aplicação? Esta ação não pode ser desfeita.')) {
      this.http.delete(this.baseUrl + 'aplicacaos/' + id).subscribe(() => this.carregar());
    }
  }

  cancelar() {
    this.modoEdicao = false;
    this.selecionado = null;
    this.configuracoes = [];
    this.marketplaceSelecionado = null;
    this.logoUrl = null;
    this.erroLogo = null;
    if (this._logoObjectUrl) { URL.revokeObjectURL(this._logoObjectUrl); this._logoObjectUrl = null; }
  }

  carregarLogo() {
    this.logoUrl = null;
    this.erroLogo = null;
    this.http.get(this.baseUrl + 'aplicacaos/logo', { responseType: 'blob' }).subscribe({
      next: blob => {
        if (this._logoObjectUrl) URL.revokeObjectURL(this._logoObjectUrl);
        this._logoObjectUrl = URL.createObjectURL(blob);
        this.logoUrl = this._logoObjectUrl;
      },
      error: () => { this.logoUrl = null; }
    });
  }

  onLogoSelecionada() {
    const file = this.logoInput?.nativeElement?.files?.[0];
    if (!file) return;
    this.erroLogo = null;
    const tiposPermitidos = ['image/png', 'image/jpeg', 'image/webp', 'image/svg+xml'];
    if (!tiposPermitidos.includes(file.type)) {
      this.erroLogo = 'Tipo não permitido. Use PNG, JPG, WebP ou SVG.';
      return;
    }
    if (file.size > 204800) {
      this.erroLogo = 'Arquivo muito grande. Limite: 200 KB.';
      return;
    }
    const formData = new FormData();
    formData.append('arquivo', file);
    this.http.post(this.baseUrl + 'aplicacaos/logo', formData).subscribe({
      next: () => this.carregarLogo(),
      error: err => { this.erroLogo = err?.error?.message || 'Erro ao enviar o logo.'; }
    });
  }

  abrirAbaSegmentos() {
    this.abaAtiva = 'segmentos';
    if (!this.segmentosDisponiveis.length) {
      this.http.get<any[]>(this.baseUrl + 'segmentos/disponiveis')
        .subscribe(r => this.segmentosDisponiveis = r);
    }
    this.carregarMeusSegmentos();
  }

  carregarMeusSegmentos() {
    this.http.get<any[]>(this.baseUrl + 'segmentos/minha-aplicacao')
      .subscribe(r => this.meusSegmentos = r.map((s: any) => s.segmentoTenantId));
  }

  temSegmento(id: string): boolean {
    return this.meusSegmentos.includes(id);
  }

  toggleSegmento(id: string) {
    if (this.temSegmento(id)) {
      this.http.delete(this.baseUrl + `segmentos/minha-aplicacao/${id}`)
        .subscribe(() => this.meusSegmentos = this.meusSegmentos.filter(s => s !== id));
    } else {
      this.http.post(this.baseUrl + `segmentos/minha-aplicacao/${id}`, {})
        .subscribe(() => this.meusSegmentos = [...this.meusSegmentos, id]);
    }
  }

  abrirAbaMarketplace() {
    this.abaAtiva = 'marketplace';
    if (!this.configuracoes.length) this.carregarConfiguracoes();
    if (this.mlConectado) {
      this.carregarConfiguracoes();
      this.mlConectado = false;
    }
  }

  conectarMercadoLivre() {
    const aplicacaoid = this.selecionado?.aplicacaoid;
    if (!aplicacaoid) return;
    window.location.href = this.baseUrl + `marketplace/ml/connect?aplicacaoid=${aplicacaoid}`;
  }

  desconectarMarketplace(marketplace: string) {
    if (!confirm(`Desconectar ${this.nomeMarketplace(marketplace)}? As credenciais serão removidas.`)) return;
    this.http.delete(this.baseUrl + `marketplace/configuracoes/${marketplace}`).subscribe({
      next: () => {
        this.marketplaceSelecionado = null;
        this.carregarConfiguracoes();
      }
    });
  }

  carregarConfiguracoes() {
    this.http.get<any[]>(this.baseUrl + 'marketplace/configuracoes')
      .subscribe(r => this.configuracoes = r);
  }

  isConfigurado(marketplaceId: string): boolean {
    return this.configuracoes.some(c => c.marketplace === marketplaceId);
  }

  nomeMarketplace(id: string): string {
    return this.marketplaces.find(m => m.id === id)?.nome ?? id;
  }

  selecionarMarketplace(id: string) {
    this.marketplaceSelecionado = id;
    this.configSalva = false;
    const existente = this.configuracoes.find(c => c.marketplace === id);
    this.credenciais = {
      accessToken: '',
      refreshToken: '',
      sellerId: existente?.sellerId ?? '',
      jaConfigurado: !!existente,
    };
  }

  salvarConfiguracao() {
    if (!this.credenciais.jaConfigurado && !this.credenciais.accessToken) return;
    this.salvandoConfig = true;
    this.configSalva = false;
    const payload: any = { marketplace: this.marketplaceSelecionado, sellerId: this.credenciais.sellerId };
    if (this.credenciais.accessToken) payload.accessToken = this.credenciais.accessToken;
    if (this.credenciais.refreshToken) payload.refreshToken = this.credenciais.refreshToken;
    this.http.post(this.baseUrl + 'marketplace/configuracoes', payload).subscribe({
      next: () => {
        this.salvandoConfig = false;
        this.configSalva = true;
        this.carregarConfiguracoes();
      },
      error: () => { this.salvandoConfig = false; }
    });
  }
}
