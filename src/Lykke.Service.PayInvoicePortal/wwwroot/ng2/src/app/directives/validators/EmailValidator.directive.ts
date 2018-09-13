// https://angular.io/guide/form-validation#custom-validators

import { Directive } from '@angular/core';
import {
  NG_VALIDATORS,
  Validator,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
  FormGroup
} from '@angular/forms';
import { isValidEmail } from 'src/app/utils/utils';

@Directive({
  selector: '[lpEmail]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: EmailValidatorDirective,
      multi: true
    }
  ]
})
export class EmailValidatorDirective implements Validator {
  validate(control: AbstractControl): ValidationErrors {
    return validateEmail(control);
  }
}

const validateEmail: ValidatorFn = (
  control: FormGroup
): ValidationErrors | null => {
  return isValidEmail(control.value) ? null : { email: true };
};
