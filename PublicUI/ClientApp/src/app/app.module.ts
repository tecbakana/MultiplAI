import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NovoOrcamentoComponent } from './orcamento/novo-orcamento.component';
import { SitePublicoComponent } from './site/site-publico.component';

@NgModule({
  declarations: [
    AppComponent,
    NovoOrcamentoComponent,
    SitePublicoComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: 'orcamento/novo', component: NovoOrcamentoComponent },
      { path: 'loja', loadChildren: () => import('./loja/loja.module').then(m => m.LojaModule) },
      { path: 's/:slug/loja', loadChildren: () => import('./loja/loja.module').then(m => m.LojaModule) },
      { path: 's/:slug', component: SitePublicoComponent },
      { path: 's/:slug/:areaurl', component: SitePublicoComponent },
      { path: '**', redirectTo: '/loja' }
    ])
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
