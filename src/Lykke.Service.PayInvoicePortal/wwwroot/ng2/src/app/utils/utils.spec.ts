import { nameof, getParameterByName, isValidEmail } from './utils';

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

describe('isValidEmail', () => {

  it('should check valid emails', (() => {
    expect(isValidEmail('valid@email.com')).toBeTruthy();
    expect(isValidEmail('valid@email.subdomain.com')).toBeTruthy();
    expect(isValidEmail('valid@111.com')).toBeTruthy();
    expect(isValidEmail('111@111.com')).toBeTruthy();
  }));

  it('should check valid emails - special symbols', (() => {
    expect(isValidEmail('special\/#?@symbols.com')).toBeTruthy();
  }));

  it('should check invalid emails - empty', (() => {
    expect(isValidEmail('')).toBeFalsy();
  }));

  it('should check invalid emails - has space', (() => {
    expect(isValidEmail(' ')).toBeFalsy();
    expect(isValidEmail(' valid@email.com')).toBeFalsy();
  }));

  it('should check invalid emails - double dots', (() => {
    expect(isValidEmail('double@dots..com')).toBeFalsy();
  }));

  it('should check invalid emails - no domain', (() => {
    expect(isValidEmail('no@domain')).toBeFalsy();
  }));

  it('should check invalid emails - last domain has less 2 symbols', (() => {
    expect(isValidEmail('test@test.a')).toBeFalsy();
  }));

  it('should check invalid emails - not only letters in last domain', (() => {
    expect(isValidEmail('test@test.111aaa')).toBeFalsy();
  }));

});
