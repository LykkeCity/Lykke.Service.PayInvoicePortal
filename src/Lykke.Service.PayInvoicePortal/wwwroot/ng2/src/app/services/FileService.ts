import { Injectable } from '@angular/core';

@Injectable()
export class FileService {
  getExtension(fileName: string): string {
    return fileName ? fileName.split('.').pop() : '';
  }

  getSize(fileSize: number): string {
    if (fileSize < 1024) {
      return fileSize + ' bytes';
    } else if (fileSize > 1024 && fileSize < 1048576) {
      return (fileSize / 1024).toFixed(0) + ' KB';
    } else {
      return (fileSize / 1048576).toFixed(0) + ' MB';
    }
  }
}
