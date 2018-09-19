import { Component, OnInit } from '@angular/core';
import { SettingsApi } from '../../services/api/SettingsApi';
import {
  ROUTE_CHANGE_PASSWORD_PAGE,
  ROUTE_WELCOME_PAGE
} from '../../constants/routes';
import { SettingsModel } from './SettingsModel';
import { AssetItemViewModel } from '../../models/AssetItemViewModels';
import { ConfirmModalService } from '../../services/ConfirmModalService';

@Component({
  selector: SettingsComponent.Selector,
  templateUrl: './Settings.html'
})
export class SettingsComponent implements OnInit {
  static readonly Selector = 'lp-settings';
  private previousBaseAssetId = '';
  model = new SettingsModel();
  view = new View();
  errors = new Errors();
  revertBaseAssetTrigger = 0;
  constructor(
    private api: SettingsApi,
    private confirmModalService: ConfirmModalService
  ) {}
  ngOnInit(): void {
    this.view.isLoading = true;

    this.api.getSettings().subscribe(
      res => {
        const settingsResponse = res as SettingsModel;
        if (!settingsResponse.baseAssetId) {
          settingsResponse.baseAssetId = '';
          settingsResponse.availableBaseAssets = [
            new AssetItemViewModel('', 'Choose asset...')
          ].concat(settingsResponse.availableBaseAssets);
        }
        this.model = settingsResponse;
        this.view.isContentLoaded = true;
        this.view.isLoading = false;
      },
      error => {
        console.error(error);
        this.view.isLoading = false;
      }
    );
  }

  onChangedBaseAssetId(newBaseAssetId): void {
    // console.log(
    //   'this.previousBaseAssetId: ' + this.previousBaseAssetId,
    //   'newBaseAssetId: ' + newBaseAssetId,
    //   'this.model.baseAssetId: ' + this.model.baseAssetId
    // );
    if (
      !newBaseAssetId ||
      newBaseAssetId === this.model.baseAssetId ||
      this.previousBaseAssetId === newBaseAssetId
    ) {
      return;
    }

    this.view.isLoadingChangeAsset = true;
    this.errors.baseAssetId = false;
    const tempPreviousBaseAssetId = this.previousBaseAssetId;
    this.previousBaseAssetId = newBaseAssetId;

    this.api.setBaseAsset({ baseAssetId: newBaseAssetId }).subscribe(
      res => {
        this.view.isLoadingChangeAsset = false;

        if (this.model.availableBaseAssets[0].id === '') {
          const newAssets = [].concat(this.model.availableBaseAssets);
          newAssets.shift();
          this.model.availableBaseAssets = newAssets;
        }
      },
      error => {
        console.error(error);
        this.previousBaseAssetId = tempPreviousBaseAssetId;
        this.revertBaseAssetTrigger++;
        this.errors.baseAssetId = true;
        this.view.isLoadingChangeAsset = false;
      }
    );
  }

  goToChangePassword(): void {
    window.location.href = ROUTE_CHANGE_PASSWORD_PAGE;
  }

  generateRsaKeys(): void {
    this.view.isLoadingGenerateRsaKeys = true;

    this.api.generateRsaKeys().subscribe(
      res => {
        this.model.rsaPrivateKey = res.rsaPrivateKey;
        this.model.hasPublicKey = true;
        this.view.showRsaPrivateKey = true;
        this.view.isLoadingGenerateRsaKeys = false;
      },
      error => {
        console.error(error);
        this.view.isLoadingGenerateRsaKeys = false;
      }
    );
  }

  deleteAccount(): void {
    this.confirmModalService.openModal({
      content: 'Are you sure to delete account?',
      yesAction: () => {
        this.view.isLoadingDeleteAccount = true;

        this.api.deleteAccount().subscribe(
          res => {
            location.href = ROUTE_WELCOME_PAGE;
          },
          error => {
            console.error(error);
            this.confirmModalService.showErrorModal();
            this.view.isLoadingDeleteAccount = false;
          }
        );
      }
    });
  }
}

class View {
  isLoading: boolean;
  isContentLoaded: boolean;
  isLoadingChangeAsset: boolean;
  isLoadingGenerateRsaKeys: boolean;
  isLoadingDeleteAccount: boolean;
  showRsaPrivateKey: boolean;
}

class Errors {
  baseAssetId: boolean;
}
