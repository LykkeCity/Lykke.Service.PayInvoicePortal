import {
  Component,
  Renderer2,
  ViewChild,
  Input,
  ElementRef,
  Output,
  EventEmitter
} from '@angular/core';
import { InvoiceEditModel } from './InvoiceEditModel';
import { AssetsApi } from 'src/app/services/api/AssetsApi';
import { AssetItemViewModel } from 'src/app/models/AssetItemViewModels';
import { ConfirmModalService } from 'src/app/services/ConfirmModalService';
import {
  InvoicesApi,
  CreateInvoiceRequest,
  UpdateInvoiceRequest
} from 'src/app/services/api/InvoicesApi';
import { FormGroup } from '@angular/forms';
import { InvoiceInfoComponent } from '../InvoiceInfo/InvoiceInfo';
import { FileService } from 'src/app/services/FileService';
import { InvoiceModel } from '../InvoiceDetails/InvoiceModel';
import { FileModel } from 'src/app/models/FileModel';
import { FilesApi } from 'src/app/services/api/FilesApi';
import { Observable } from 'rxjs';

@Component({
  selector: InvoiceEditComponent.Selector,
  templateUrl: './InvoiceEdit.html'
})
export class InvoiceEditComponent implements IInvoiceEditComponentHandlers {
  static readonly Selector = 'lp-invoice-edit';

  @Output()
  invoiceUpdated = new EventEmitter();

  @Output()
  filesUpdated = new EventEmitter<FileModel[]>();

  @ViewChild(InvoiceInfoComponent)
  invoiceInfoSidebar: InvoiceInfoComponent;

  @ViewChild('invoiceForm')
  invoiceForm: FormGroup;

  model = new InvoiceEditModel();
  view = new View();
  validation = new Validation();

  constructor(
    private confirmModalService: ConfirmModalService,
    private fileService: FileService,
    private assetsApi: AssetsApi,
    private invoicesApi: InvoicesApi,
    private filesApi: FilesApi,
    private renderer: Renderer2
  ) {}

  open(model?: InvoiceEditModel): void {
    this.reset();

    if (model) {
      this.model.isNewInvoice = false;
      this.model.copyFrom(model);
    }

    this.renderer.setStyle(document.body, 'overflow', 'hidden');
    this.view.show = true;

    if (this.model.settlementAssets.length === 0) {
      this.view.isLoadingAssets = true;
      this.assetsApi.getSettlementAssets().subscribe(
        (res: Array<AssetItemViewModel>) => {
          if (res && res.length) {
            this.model.selectedSettlementAssetId = res[0].id;
            this.model.settlementAssets = res;
          }

          this.view.isLoadingAssets = false;
        },
        error => {
          console.error(error);
          this.confirmModalService.showErrorModal();
          this.view.isLoadingAssets = false;
        }
      );
    }
  }

  close(): void {
    this.renderer.removeStyle(document.body, 'overflow');
    this.view.show = false;
  }

  overlayClick(e: MouseEvent): void {
    if (this.view.show) {
      e.preventDefault();
      e.stopPropagation();

      this.close();
    }
  }

  getAcceptFilesExtensions(): string {
    return this.fileService.getAcceptFilesExtensions();
  }

  getFileExtension(fileName: string): string {
    return this.fileService.getExtension(fileName);
  }

  getFileSize(fileSize: number): string {
    return this.fileService.getSize(fileSize);
  }

  addFiles(files: FileList): void {
    if (!files || files.length === 0) {
      return;
    }

    let isValid = true;
    for (let i = 0; i < files.length; i++) {
      isValid = this.fileService.validate(files[i]);
      if (!isValid) {
        break;
      }
    }

    if (isValid) {
      if (this.model.isNewInvoice) {
        for (let i = 0; i < files.length; i++) {
          (<File[]>this.model.files).push(files[i]);
        }
      } else {
        this.view.isUploadingFiles = true;

        this.filesApi.uploadFiles(this.model.id, files).subscribe(
          res => {
            this.view.isUploadingFiles = false;
            this.updateFiles();
          },
          error => {
            console.error(error);
            this.confirmModalService.showErrorModal();
            this.view.isUploadingFiles = false;
          }
        );
      }
    } else {
      this.confirmModalService.openModal({
        title: 'Invalid file',
        content: this.fileService.getError()
      });
    }
  }

  getFile(file: FileModel): void {
    window.open(`/api/files/${file.id}/${this.model.id}`);
  }

  deleteFile(index: number): void {
    if (index >= 0) {
      if (this.model.isNewInvoice) {
        this.model.files.splice(index, 1);
      } else {
        this.confirmModalService.openModal({
          content: `Are you sure you want to remove this attachment "${
            this.model.files[index].name
          }"?`,
          yesAction: () => {
            this.filesApi
              .deleteFile(
                (this.model.files[index] as FileModel).id,
                this.model.id
              )
              .subscribe(
                res => {
                  this.updateFiles();
                },
                error => {
                  console.error(error);
                  this.confirmModalService.showErrorModal();
                }
              );
          }
        });
      }
    }
  }

  saveDraft(): void {
    this.save(true);
  }

  generate(): void {
    this.save(false);
  }

  private save(isDraft: boolean): void {
    if (this.invoiceForm.invalid) {
      this.validation.isSubmitPressed = true;
      return;
    }

    this.view.isLoading = true;

    const createInvoiceRequest = new CreateInvoiceRequest(
      isDraft,
      this.model.number,
      this.model.client,
      this.model.email,
      this.model.amount,
      this.model.selectedSettlementAssetId,
      (this.model.dueDate as any).endOf('day').toISOString(),
      this.model.note
    );

    let observableApiMethod: Observable<any> = null;

    if (this.model.isNewInvoice) {
      observableApiMethod = this.invoicesApi.add(createInvoiceRequest, <File[]>(
        this.model.files
      ));
    } else {
      const updateInvoiceRequest = createInvoiceRequest as UpdateInvoiceRequest;
      updateInvoiceRequest.id = this.model.id;
      observableApiMethod = this.invoicesApi.update(updateInvoiceRequest);
    }

    observableApiMethod.subscribe(
      (res: InvoiceModel) => {
        this.view.isLoading = false;

        this.close();
        if (!isDraft) {
          this.invoiceInfoSidebar.open(res);
        }

        if (!this.model.isNewInvoice) {
          this.invoiceUpdated.emit();
        }
      },
      error => {
        console.error(error);
        this.confirmModalService.showErrorModal();
        this.view.isLoading = false;
      }
    );
  }

  private updateFiles(): void {
    this.filesApi.getFiles(this.model.id).subscribe(
      (res: FileModel[]) => {
        this.model.files = res;
        this.filesUpdated.emit([...res]);
      },
      error => {
        console.error(error);
        this.confirmModalService.showErrorModal();
      }
    );
  }

  private reset(): void {
    for (const key in this.invoiceForm.controls) {
      if (this.invoiceForm.controls.hasOwnProperty(key)) {
        const control = this.invoiceForm.controls[key];
        control.markAsPristine();
        control.markAsUntouched();
      }
    }
    this.validation.reset();
    this.model.reset();
  }
}

interface IInvoiceEditComponentHandlers {
  open: (_?: InvoiceEditModel) => void;
  close: () => void;
  overlayClick: (_: MouseEvent) => void;
  getAcceptFilesExtensions: () => string;
  getFileExtension: (_: string) => string;
  getFileSize: (_: number) => string;
  addFiles: (files: FileList) => void;
  getFile: (file: FileModel) => void;
  deleteFile: (index: number) => void;
  saveDraft: () => void;
  generate: () => void;
}

class View {
  show: boolean;
  isLoadingAssets: boolean;
  isUploadingFiles: boolean;
  isLoading: boolean;
}

class Validation {
  isSubmitPressed: boolean;

  reset(): void {
    this.isSubmitPressed = false;
  }
}
