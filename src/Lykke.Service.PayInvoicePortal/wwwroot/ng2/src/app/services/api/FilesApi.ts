import { BaseApi } from './BaseApi';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { nameof } from 'src/app/utils/utils';

@Injectable()
export class FilesApi extends BaseApi {

  getFiles(invoiceId: string): Observable<any> {
    return this.get(`api/files/${invoiceId}`);
  }

  uploadFiles(invoiceId: string, files: FileList): Observable<any> {
    const formData = new FormData();

    formData.append('invoiceId', invoiceId);

    if (files && files.length) {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        formData.append('files', file, file.name);
      }
    }

    return this.post('api/files', formData);
  }

  deleteFile(fileId: string, invoiceId: string): Observable<any> {
    return this.delete(`api/files/${fileId}?invoiceId=${invoiceId}`);
  }
}
