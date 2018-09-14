import { Component, OnInit } from '@angular/core';
import { SettingsApi } from '../../services/api/SettingsApi';
import { ROUTE_CHANGE_PASSWORD_PAGE } from '../../constants/routes';
import { SettingsModel } from './SettingsModel';

@Component({
  selector: SettingsComponent.Selector,
  templateUrl: './Settings.html'
})
export class SettingsComponent implements OnInit {
  static readonly Selector = 'lp-settings';
  model = new SettingsModel();
  view = new View();
  constructor(private api: SettingsApi) {}
  ngOnInit(): void {
    this.view.isLoading = true;

    this.api.getSettings().subscribe(
      res => {
        this.model = res as SettingsModel;
        this.view.isContentLoaded = true;
        this.view.isLoading = false;
      },
      error => {
        console.error(error);
        this.view.isLoading = false;
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
}

class View {
  isLoading: boolean;
  isContentLoaded: boolean;
  isLoadingChangeAsset: boolean;
  isLoadingGenerateRsaKeys: boolean;
  isLoadingDeleteAccount: boolean;
  showRsaPrivateKey: boolean;
}
