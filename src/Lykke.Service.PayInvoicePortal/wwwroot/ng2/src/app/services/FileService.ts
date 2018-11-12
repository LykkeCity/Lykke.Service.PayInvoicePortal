import { Injectable } from '@angular/core';

@Injectable()
export class FileService {
  private readonly defaultMaxSizeInMB = 5;

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

  getAcceptFilesExtensions(): string {
    return '.jpg,.jpeg,.png,.pdf,.doc,.docx,.xls,.xlsx,.rtf';
  }

  validate(file: File, maxSizeInMB: number = this.defaultMaxSizeInMB) {
    if (!file) {
      return false;
    }

    if (file.size === 0 || file.size > maxSizeInMB * 1048576) {
      return false;
    }

    const extension = this.getExtension(file.name);

    if (!extension) {
      return false;
    }

    const validExtensions = this.getAcceptFilesExtensions().split(',');

    return validExtensions.indexOf(`.${extension.toLowerCase()}`) >= 0;
  }

  getError() {
    return `One or more files are invalid.<br>
            Please check the requirements:
            <ul>
              <li>• the maximum file size is ${this.defaultMaxSizeInMB} MB</li>
              <li>• allowed types: ${this.getAcceptFilesExtensions()}</li>
            </ul>`;
  }
}
