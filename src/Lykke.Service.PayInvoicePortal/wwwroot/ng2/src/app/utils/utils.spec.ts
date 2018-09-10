import { nameof, getParameterByName } from './utils';

describe('nameof', () => {

  it(`should return property string`, (() => {
    const property = '';
    const res = nameof(() => this.property);
    expect(res).toEqual('property');
  }));

});

describe('getParameterByName', () => {

  it(`should return parameter value from url by name`, (() => {
    const parameterName = 'token';
    const parameterValue = '123';
    const url = `http://localhost/anypath?${parameterName}=${parameterValue}`;
    const res = getParameterByName(parameterName, url);
    expect(res).toEqual(parameterValue);
  }));

  it(`should return empty value from url by wrong name`, (() => {
    const url = `http://localhost/anypath?anyparam=anyvalue`;
    const res = getParameterByName('wrongParam', url);
    expect(res).toBeNull();
  }));

});
