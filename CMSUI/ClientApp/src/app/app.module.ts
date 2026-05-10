import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AuthInterceptor } from './auth.interceptor';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { UsuarioComponent } from './usuario/usuario.component';
import { AplicacaoComponent } from './aplicacao/aplicacao.component';
import { ConteudoComponent } from './conteudo/conteudo.component';
import { AreaComponent } from './area/area.component';
import { CategoriaComponent } from './categoria/categoria.component';
import { EmConstrucaoComponent } from './em-construcao/em-construcao.component';
import { ProdutoComponent } from './produto/produto.component';
import { FormularioComponent } from './formulario/formulario.component';
import { SignupComponent } from './signup/signup.component';
import { GrupoComponent } from './grupo/grupo.component';
import { VinculoComponent } from './vinculo/vinculo.component';
import { VinculoModuloComponent } from './vinculo-modulo/vinculo-modulo.component';
import { PageBuilderComponent } from './page-builder/page-builder.component';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { SiteComponent } from './site/site.component';
import { LandingComponent } from './landing/landing.component';
import { PedidoComponent } from './pedido/pedido.component';
import { OrcamentoComponent } from './orcamento/orcamento.component';
import { NovoOrcamentoComponent } from './orcamento/novo-orcamento.component';
import { ModuloComponent } from './modulo/modulo.component';
@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    LoginComponent,
    UsuarioComponent,
    AplicacaoComponent,
    ConteudoComponent,
    AreaComponent,
    CategoriaComponent,
    EmConstrucaoComponent,
    ProdutoComponent,
    FormularioComponent,
    SignupComponent,
    GrupoComponent,
    VinculoComponent,
    VinculoModuloComponent,
    PageBuilderComponent,
    SiteComponent,
    LandingComponent,
    PedidoComponent,
    OrcamentoComponent,
    NovoOrcamentoComponent,
    ModuloComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    DragDropModule,
    RouterModule.forRoot([
      { path: '', component: LandingComponent, pathMatch: 'full' },
      { path: 'dashboard', component: HomeComponent },
      { path: 'login', component: LoginComponent },
      { path: 'signup', component: SignupComponent },
      { path: 'usuario', component: UsuarioComponent },
      { path: 'usuarios', component: UsuarioComponent },
      { path: 'aplicacao', component: AplicacaoComponent },
      { path: 'aplicacoes', component: AplicacaoComponent },
      { path: 'minha-aplicacao', component: AplicacaoComponent },
      { path: 'conteudo', component: ConteudoComponent },
      { path: 'area', component: AreaComponent },
      { path: 'areas', component: AreaComponent },
      { path: 'categoria', component: CategoriaComponent },
      { path: 'categorias', component: CategoriaComponent },
      { path: 'produtos', component: ProdutoComponent },
      { path: 'formularios', component: FormularioComponent },
      { path: 'grupos', component: GrupoComponent },
      { path: 'vinculos', component: VinculoComponent },
      { path: 'modulos-admin', component: ModuloComponent },
      { path: 'vinculosmodulo', component: VinculoModuloComponent },
      { path: 'pedidos', component: PedidoComponent },
      { path: 'orcamentos', component: OrcamentoComponent },
      { path: 'orcamento/novo', component: NovoOrcamentoComponent },
      { path: 'marketplace', loadChildren: () => import('./marketplace/marketplace.module').then(m => m.MarketplaceModule) },
      { path: 'loja', loadChildren: () => import('./loja/loja.module').then(m => m.LojaModule) },
      { path: 's/:slug/loja', loadChildren: () => import('./loja/loja.module').then(m => m.LojaModule) },
      { path: 'page-builder', component: PageBuilderComponent },
      { path: 'page-builder-v2', loadChildren: () => import('./page-builder-v2/page-builder-v2.module').then(m => m.PageBuilderV2Module) },
      { path: 's/:slug', component: SiteComponent },
      { path: 's/:slug/:area', component: SiteComponent },
      { path: 'preview/:id', component: SiteComponent },
      { path: 'preview/:id/:area', component: SiteComponent },
      { path: '**', component: EmConstrucaoComponent }
    ])
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
