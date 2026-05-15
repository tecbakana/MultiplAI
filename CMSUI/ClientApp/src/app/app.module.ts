import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AuthInterceptor } from './auth.interceptor';
import { AuthGuard } from './auth.guard';

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
import { SegmentoComponent } from './segmento/segmento.component';
import { VitrineComponent } from './vitrine/vitrine.component';
import { VitrineAdminComponent } from './vitrine-admin/vitrine-admin.component';
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
    ModuloComponent,
    SegmentoComponent,
    VitrineComponent,
    VitrineAdminComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    DragDropModule,
    RouterModule.forRoot([
      { path: '', component: LandingComponent, pathMatch: 'full' },
      { path: 'dashboard', component: HomeComponent, canActivate: [AuthGuard] },
      { path: 'login', component: LoginComponent },
      { path: 'signup', component: SignupComponent },
      { path: 'usuario', component: UsuarioComponent, canActivate: [AuthGuard] },
      { path: 'usuarios', component: UsuarioComponent, canActivate: [AuthGuard] },
      { path: 'aplicacao', component: AplicacaoComponent, canActivate: [AuthGuard] },
      { path: 'aplicacoes', component: AplicacaoComponent, canActivate: [AuthGuard] },
      { path: 'minha-aplicacao', component: AplicacaoComponent, canActivate: [AuthGuard] },
      { path: 'conteudo', component: ConteudoComponent, canActivate: [AuthGuard] },
      { path: 'area', component: AreaComponent, canActivate: [AuthGuard] },
      { path: 'areas', component: AreaComponent, canActivate: [AuthGuard] },
      { path: 'categoria', component: CategoriaComponent, canActivate: [AuthGuard] },
      { path: 'categorias', component: CategoriaComponent, canActivate: [AuthGuard] },
      { path: 'produtos', component: ProdutoComponent, canActivate: [AuthGuard] },
      { path: 'formularios', component: FormularioComponent, canActivate: [AuthGuard] },
      { path: 'grupos', component: GrupoComponent, canActivate: [AuthGuard] },
      { path: 'vinculos', component: VinculoComponent, canActivate: [AuthGuard] },
      { path: 'modulos-admin', component: ModuloComponent, canActivate: [AuthGuard] },
      { path: 'segmentos', component: SegmentoComponent, canActivate: [AuthGuard] },
      { path: 'vitrine-admin', component: VitrineAdminComponent, canActivate: [AuthGuard] },
      { path: 'vitrine', component: VitrineComponent, canActivate: [AuthGuard] },
      { path: 'vitrine/:areaid', redirectTo: 'vitrine', pathMatch: 'full' },
      { path: 'vinculosmodulo', component: VinculoModuloComponent, canActivate: [AuthGuard] },
      { path: 'pedidos', component: PedidoComponent, canActivate: [AuthGuard] },
      { path: 'orcamentos', component: OrcamentoComponent, canActivate: [AuthGuard] },
      { path: 'orcamento/novo', component: NovoOrcamentoComponent, canActivate: [AuthGuard] },
      { path: 'marketplace', loadChildren: () => import('./marketplace/marketplace.module').then(m => m.MarketplaceModule), canActivate: [AuthGuard] },
      { path: 'page-builder', component: PageBuilderComponent, canActivate: [AuthGuard] },
      { path: 'page-builder-v2', loadChildren: () => import('./page-builder-v2/page-builder-v2.module').then(m => m.PageBuilderV2Module), canActivate: [AuthGuard] },
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
