import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CarrinhoService, ItemCarrinho } from '../services/carrinho.service';
import { LojaApiService, NovoPedido, ClienteLoja } from '../services/loja-api.service';
import { LojaAuthService } from '../services/loja-auth.service';
import { LojaContextService } from '../services/loja-context.service';

@Component({ templateUrl: './checkout.component.html' })
export class CheckoutComponent implements OnInit {
  itens: ItemCarrinho[] = [];
  clienteSelecionado: ClienteLoja | null = null;
  metodoPagamento = '';
  enviando = false;
  erro = '';
  sucesso = false;
  numeroPedidoCriado = '';

  readonly metodosPagamento = ['pix', 'cartao_credito', 'cartao_debito', 'boleto'];

  constructor(
    private carrinho: CarrinhoService,
    private api: LojaApiService,
    private auth: LojaAuthService,
    private router: Router,
    private ctx: LojaContextService
  ) {}

  get cliente() {
    return this.clienteSelecionado ?? this.auth.auth;
  }

  ngOnInit() {
    this.clienteSelecionado = (history.state as any)['clienteSelecionado'] ?? null;
    this.itens = this.carrinho.getItens();
    if (this.itens.length === 0) {
      const base = this.ctx.slug ? `/s/${this.ctx.slug}/loja` : '/loja'
      this.router.navigate([`${base}/carrinho`]);
    }
  }

  total(): number {
    return this.carrinho.total();
  }

  voltar() {
    const base = this.ctx.slug ? `/s/${this.ctx.slug}/loja` : '/loja'
    this.router.navigate([`${base}/catalogo`]);
  }

  podeFinalizar(): boolean {
    return !!this.metodoPagamento && !this.enviando;
  }

  finalizar() {
    if (!this.podeFinalizar() || !this.cliente) return;

    this.enviando = true;
    this.erro = '';

    const pedido: NovoPedido = {
      token: this.ctx.token,
      numeropedido: `PED-${Date.now()}`,
      clientenome: this.cliente.nome,
      clienteemail: this.cliente.email,
      valorpedido: this.total(),
      metodoPagamento: this.metodoPagamento,
      itens: this.itens.map(i => ({
        produtoid: i.produtoid,
        sku: i.sku,
        nome: i.nome,
        quantidade: i.quantidade,
        valorUnitario: i.valor
      }))
    };

    this.api.criarPedido(pedido).subscribe({
      next: (r) => {
        this.numeroPedidoCriado = r.numeropedido;
        this.carrinho.limpar();
        this.sucesso = true;
        this.enviando = false;
      },
      error: () => {
        this.erro = 'Falha ao criar pedido. Tente novamente.';
        this.enviando = false;
      }
    });
  }

  formatarValor(v: number): string {
    return v?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) ?? '—';
  }

  formatarMetodo(m: string): string {
    const labels: Record<string, string> = {
      pix: 'PIX',
      cartao_credito: 'Cartão de Crédito',
      cartao_debito: 'Cartão de Débito',
      boleto: 'Boleto'
    };
    return labels[m] ?? m;
  }
}

