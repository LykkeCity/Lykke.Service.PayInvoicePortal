import { IItemViewModel } from './IItemViewModel';

export class AssetItemViewModel implements IItemViewModel {
  id: string;
  title: string;
  constructor(id, title) {
    this.id = id;
    this.title = title;
  }
}
