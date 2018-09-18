/** Returns the name of a namespace or variable reference at runtime. */
export const nameof = (selector: () => any, fullname = false) => {
  const s = '' + selector;
  const m =
    s.match(/return\s+([A-Z$_.]+)/i) ||
    s.match(/.*?(?:=>|function.*?{(?!\s*return))\s*([A-Z$_.]+)/i);
  const name = (m && m[1]) || '';
  const result = fullname ? name : name.split('.').reverse()[0];

  if (!result) {
    console.error(`null nameof: ${selector}`, name);
  }

  return result;
};

export const getParameterByName = (name: string, url: string): string => {
  if (!url) {
    url = window.location.href;
  }
  name = name.replace(/[\[\]]/g, '\\$&');
  const regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)');
  const results = regex.exec(url);
  if (!results) {
    return null;
  }
  if (!results[2]) {
    return null;
  }
  return decodeURIComponent(results[2].replace(/\+/g, ' '));
};

export const isValidEmail = (email: string): boolean => {
  if (!email) {
    return false;
  }

  if (email.indexOf(' ') > -1) {
    return false;
  }

  const lines = email.split('@');

  if (lines.length !== 2) {
    return false;
  }

  if (lines[1].indexOf('..') > -1) {
    return false;
  }

  const lines2 = lines[1].split('.');

  if (lines2.length < 2) {
    return false;
  }

  const lastDomain = lines2.reverse()[0];

  const result = /^[A-z]{2,}$/.test(lastDomain);

  return result;
};
