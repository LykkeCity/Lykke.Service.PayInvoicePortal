import { TestBed, async } from '@angular/core/testing';
import { nameof } from './utils';

describe('nameof', () => {

  it(`should return property string`, async(() => {
    const property = '';
    const res = nameof(() => this.property);
    expect(res).toEqual('property');
  }));

});
