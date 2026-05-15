import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LojaAuthResponse } from './loja-auth.service';

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface RegistrarRequest {
  token: string;
  nome: string;
  documento: string;
  email: string;
  senha: string;
  telefone: string;
  cep: string;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  estado: string;
}

export interface Produto {
  produtoid: string;
  sku: string;
  nome: string;
  descricacurta: string;
  valor: number;
}

export interface ClienteLoja {
  salematicClienteId: number;
  nome: string;
  documento: string;
  email: string;
  telefone: string;
}

export interface NovoPedido {
  token: string;
  numeropedido: string;
  clientenome: string;
  clienteemail: string;
  valorpedido: number;
  metodoPagamento: string;
  itens: { produtoid: string; sku: string; nome: string; quantidade: number; valorUnitario: number }[];
}

@Injectable()
export class LojaApiService {

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  getProdutos(token: string): Observable<Produto[]> {
    const p = new HttpParams().set('token', token);
    return this.http.get<Produto[]>(this.baseUrl + 'api/publico/loja/catalogo', { params: p });
  }

  getClientes(): Observable<ClienteLoja[]> {
    return this.http.get<ClienteLoja[]>(this.baseUrl + 'api/publico/loja/clientes');
  }

  cadastrarCliente(cliente: Omit<ClienteLoja, 'salematicClienteId'>): Observable<ClienteLoja> {
    return this.http.post<ClienteLoja>(this.baseUrl + 'api/publico/loja/clientes', cliente);
  }

  criarPedido(pedido: NovoPedido): Observable<any> {
    return this.http.post(this.baseUrl + 'api/publico/loja/pedidos', pedido);
  }

  getPedidos(params?: { aplicacaoid?: string; status?: string }): Observable<any[]> {
    let p = new HttpParams();
    if (params?.aplicacaoid) p = p.set('aplicacaoid', params.aplicacaoid);
    if (params?.status) p = p.set('status', params.status);
    return this.http.get<any[]>(this.baseUrl + 'api/publico/loja/pedidos', { params: p });
  }

  getMeusPedidos(email: string): Observable<any[]> {
    const p = new HttpParams().set('email', email);
    return this.http.get<any[]>(this.baseUrl + 'api/publico/loja/meus-pedidos', { params: p });
  }

  getTimeline(pedidoid: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}api/publico/loja/pedidos/${pedidoid}/timeline`);
  }

  login(req: LoginRequest): Observable<LojaAuthResponse> {
    return this.http.post<LojaAuthResponse>(this.baseUrl + 'api/publico/loja/auth/login', req);
  }

  registrar(req: RegistrarRequest): Observable<LojaAuthResponse> {
    return this.http.post<LojaAuthResponse>(this.baseUrl + 'api/publico/loja/auth/registrar', req);
  }

  esqueciSenha(req: { email: string }): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(this.baseUrl + 'api/publico/loja/auth/esqueci-senha', req);
  }

  resetSenha(req: { token: string; novaSenha: string }): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(this.baseUrl + 'api/publico/loja/auth/reset-senha', req);
  }

  consultarCep(cep: string) {
    return this.http.get(`https://viacep.com.br/ws/${cep.replace(/\D/g, '')}/json/`);
  }
}
