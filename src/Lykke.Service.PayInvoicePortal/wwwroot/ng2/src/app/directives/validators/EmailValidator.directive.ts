// https://angular.io/guide/form-validation#custom-validators

import { Directive, Input } from '@angular/core';
import {
  NG_VALIDATORS,
  Validator,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
  FormGroup
} from '@angular/forms';
import { isValidEmail } from 'src/app/utils/utils';

const defaultSeparator = ';';

@Directive({
  selector: '[lpEmailValidator]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: EmailValidatorDirective,
      multi: true
    }
  ]
})
export class EmailValidatorDirective implements Validator {
  @Input('lpEmailValidatorMultiple')
  isMultiple: boolean;

  validate(control: AbstractControl): ValidationErrors {
    return validateEmail(this.isMultiple)(control);
  }
}

function validateEmail(isMultiple: boolean): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (control.value && isMultiple) {
      const emails = (control.value as string).split(defaultSeparator);

      for (let i = 0; i < emails.length; i++) {
        if (!isValidEmail(emails[i])) {
          return { email: true };
        }
      }

      return null;
    }

    return isValidEmail(control.value) ? null : { email: true };
  };
}
