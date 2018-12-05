import { IItemViewModel } from './IItemViewModel';

export class AssetItemViewModel implements IItemViewModel {
  id: string;
  title: string;
  network = '';
  constructor(id, title) {
    this.id = id;
    this.title = title;
  }
}
