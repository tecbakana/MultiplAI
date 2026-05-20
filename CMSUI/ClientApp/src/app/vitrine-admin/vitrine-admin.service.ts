import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface VitrineAreaConfigResumo {
  vitrineTemplateId: string | null;
  templateHtmlCss: string | null;
  templateVariaveisJson: string | null;
  valoresJson: string | null;
  publicado: boolean;
}

export interface VitrineAreaConfigInput {
  vitrineTemplateId: string;
  valoresJson: string;
}

export interface VitrineGerarAreaInput {
  prompt: string;
  tipo?: string;
  estilo?: string;
  paleta?: string;
  temaCanonicoJson?: string;
}

export interface AreaSite {
  areaid: string;
  nome: string;
  url: string;
  canonicalArea: boolean;
}

@Injectable({ providedIn: 'root' })
export class VitrineAdminService {
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {}

  getAreaConfig(areaId: string): Observable<VitrineAreaConfigResumo> {
    return this.http.get<VitrineAreaConfigResumo>(this.baseUrl + `vitrine/area/${areaId}/configurada`);
  }

  salvarAreaConfig(areaId: string, input: VitrineAreaConfigInput): Observable<void> {
    return this.http.put<void>(this.baseUrl + `vitrine/area/${areaId}/configurada`, input);
  }

  publicarArea(areaId: string): Observable<void> {
    return this.http.post<void>(this.baseUrl + `vitrine/area/${areaId}/publicar`, {});
  }

  renderArea(areaId: string): Observable<string> {
    return this.http.get(this.baseUrl + `vitrine/area/${areaId}/render`, { responseType: 'text' });
  }

  gerarArea(areaId: string, input: VitrineGerarAreaInput): Observable<void> {
    return this.http.post<void>(this.baseUrl + `vitrine/area/${areaId}/gerar`, input);
  }

  getSiteAreas(aplicacaoid: string): Observable<AreaSite[]> {
    return this.http.get<{ areas: AreaSite[] }>(this.baseUrl + `site/preview/${aplicacaoid}`).pipe(
      map(r => r.areas ?? [])
    );
  }
}
