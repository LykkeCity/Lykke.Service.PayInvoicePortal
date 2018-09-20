import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable()
export class BaseApi {
  constructor(private http: HttpClient) {}

  protected readonly baseApiUrl: string = environment.production
    ? '/'
    : 'http://localhost:54081/';

  private readonly httpOptions = {
    headers: new HttpHeaders(),
    withCredentials: environment.production ? false : true,
    params: new HttpParams()
  };

  protected get(url, params?): Observable<any> {
    this.httpOptions.params = null;
    if (params) {
      this.httpOptions.params = params;
    }

    return this.http.get(this.url(url), this.httpOptions);
  }

  protected put(url, model?): Observable<any> {
    return this.http.put(this.url(url), model, this.httpOptions);
  }

  protected post(url, model?): Observable<any> {
    return this.http.post(this.url(url), model, this.httpOptions);
  }

  protected delete(url, params?): Observable<any> {
    this.httpOptions.params = null;
    if (params) {
      this.httpOptions.params = params;
    }

    return this.http.delete(this.url(url), this.httpOptions);
  }

  private url(url: string) {
    return `${this.baseApiUrl}${url}`;
  }
}
