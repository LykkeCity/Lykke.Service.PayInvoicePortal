import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable()
export class BaseApi {
  constructor(private http: HttpClient) {}

  readonly baseApiUrl: string = environment.production ? '/' : 'http://localhost:54081/';

  readonly httpOptions = {
    headers: new HttpHeaders(),
    withCredentials: environment.production ? false : true,
    params: new HttpParams()
  };

  get(url, params?): Observable<any> {
    if (params) {
      this.httpOptions.params = params;
    }

    return this.http.get(url, this.httpOptions);
  }

  put(url, model): Observable<any> {
    return this.http.put(this.url(url), model, this.httpOptions);
  }

  post(url, model): Observable<any> {
    return this.http.post(this.url(url), model, this.httpOptions);
  }

  private url(url: string) {
    return `${this.baseApiUrl}${url}`;
  }
}
