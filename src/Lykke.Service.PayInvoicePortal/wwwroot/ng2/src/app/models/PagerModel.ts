export class PagerModel {
  pageSize: number;
  page: number;
  constructor(pageSize: number) {
    this.pageSize = pageSize;
    this.page = 1;
  }
}
