import { ConfirmModalService } from '../../services/ConfirmModalService';
import { Component, OnDestroy, Renderer2, OnInit } from '@angular/core';
import { ConfirmModalModel } from './ConfirmModalModel';
import { IConfirmModalDataModel } from '../../models/IConfirmModalDataModel';
import { Subscription } from 'rxjs';
import { timeout } from 'q';

@Component({
  selector: ConfirmModalComponent.Selector,
  templateUrl: './ConfirmModal.html'
})
export class ConfirmModalComponent implements OnInit, OnDestroy {
  static readonly Selector = 'lp-confirm-modal';
  private confirmModalServiceSubscription: Subscription;
  private readonly defaultTitle = 'Please confirm';
  private readonly defaultTextYesBtn = 'Yes';
  private readonly cssClassForBody = 'body--menu_opened';
  private yesAction: () => void = null;
  private closeAction: () => void = null;

  model = new ConfirmModalModel();
  view = new View();

  handlers = {
    yes: this.yes,
    close: this.close,
    overlayClick: this.overlayClick
  };

  constructor(
    private confirmModalService: ConfirmModalService,
    private renderer: Renderer2
  ) {}

  ngOnInit(): void {
    this.confirmModalServiceSubscription = this.confirmModalService
      .getObservable()
      .subscribe(data => this.openModal(data));
  }

  ngOnDestroy() {
    this.confirmModalServiceSubscription.unsubscribe();
  }

  private yes() {
    this.yesAction();
    this.closeModal();
  }

  private close(): void {
    this.closeModal();
    if (this.closeAction) {
      this.closeAction();
    }
  }

  private overlayClick(e: MouseEvent): void {
    if (this.view.open) {
      e.preventDefault();
      e.stopPropagation();

      this.close();
    }
  }

  private openModal(data: IConfirmModalDataModel): void {
    this.renderer.addClass(document.body, this.cssClassForBody);
    this.model.title = data.title || this.defaultTitle;
    this.model.content = data.content;
    this.model.textYesBtn = data.textYesBtn || this.defaultTextYesBtn;

    if (data.yesAction) {
      this.view.showYesBtn = true;
      this.yesAction = data.yesAction || null;
      this.closeAction = data.closeAction || null;
    } else {
      this.view.showYesBtn = false;
    }

    // avoid ExpressionChangedAfterItHasBeenCheckedError: Expression has changed after it was checked. Previous value: 'undefined'
    setTimeout(() => this.view.open = true);
  }

  private closeModal(): void {
    if (this.view.open) {
      this.renderer.removeClass(document.body, this.cssClassForBody);
      this.view.open = false;
    }
  }
}

class View {
  open: boolean;
  showYesBtn: boolean;
}
